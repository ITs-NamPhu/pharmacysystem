"""Runner evaluation cho AI Agent nhà thuốc.

Cách dùng (chạy từ thư mục ai-agent):
    # chạy mock data, không dùng judge
    python -m eval.run_eval --mode mock --provider auto

    # chạy live backend, có LLM-as-judge
    export EVAL_TOKEN=...   # token backend thật
    python -m eval.run_eval --mode live --provider groq --judge

Với mode=mock: backend fixtured deterministic => số liệu trong câu trả lời có thể
đối chiếu để tìm hallucination. LLM vẫn là LLM thật (cần GROQ_API_KEY hoặc
GEMINI_API_KEY tùy provider).
"""
import argparse
import asyncio
import contextlib
import json
import logging
import os
import pathlib
import time
import traceback
from datetime import datetime

from langchain_core.messages import HumanMessage

from app.agents.pharmacy_agent import create_pharmacy_agent
from app.models.chat import AuthContext
from app.services.llm_service import create_llm_for_query
from eval.fixtures.backend_mock import mock_backend
from eval.golden import load_cases
from eval.judges import judge_answer
from eval.metrics import hallucinated_numbers, tool_f1
from eval.report import write_report

logger = logging.getLogger(__name__)

ROOT = pathlib.Path(__file__).resolve().parent.parent


def _chunk_text(content) -> str:
    """Trích text từ AIMessageChunk.content (có thể là str hoặc list ContentBlock như Gemini)."""
    if isinstance(content, str):
        return content
    if isinstance(content, list):
        parts = []
        for block in content:
            if isinstance(block, dict):
                body = block.get("text") or block.get("thought")
                if isinstance(body, str):
                    parts.append(body)
            elif isinstance(block, str):
                parts.append(block)
        return "".join(parts)
    return ""


@contextlib.asynccontextmanager
async def _noop():
    yield


def parse_args():
    p = argparse.ArgumentParser(description="Chạy evaluation cho AI Agent nhà thuốc")
    p.add_argument("--mode", choices=["mock", "live"], default="mock",
                   help="mock: dữ liệu backend giả định | live: gọi backend thật")
    p.add_argument("--provider", choices=["auto", "groq", "gemini"], default="auto",
                   help="provider LLM sẽ dùng (auto = router theo câu hỏi)")
    p.add_argument("--judge", action="store_true",
                   help="chấm điểm bằng LLM-as-judge (cần GROQ_API_KEY)")
    p.add_argument("--limit", type=int, default=0, help="chỉ chạy N case đầu (mặc định: tất cả)")
    p.add_argument("-o", "--out", default=str(ROOT / "reports"),
                   help="thư mục ghi báo cáo (mặc định ai-agent/reports)")
    return p.parse_args()


def auth_from_env() -> AuthContext:
    token = os.getenv("EVAL_TOKEN", "eval-token")
    branch_id = os.getenv("EVAL_BRANCH_ID")
    return AuthContext(token=token, branch_id=branch_id)


async def run_case(case: dict, provider: str, auth: AuthContext, judge: bool = False) -> dict:
    rec = {
        "id": case["id"],
        "category": case["category"],
        "question": case["question"],
        "expected_tools": list(case.get("expected_tools") or []),
        "provider": provider,
        "called_tools": [],
        "tool_calls": [],
        "tool_outputs": [],
        "answer": "",
        "latency_ms": None,
        "tokens": None,
        "error": None,
        "judge_score": None,
        "judge_reason": None,
        "hallucinated_numbers": [],
    }
    try:
        llm = create_llm_for_query(case["question"], provider)
    except Exception as e:  # noqa: BLE001
        rec["error"] = f"create_llm: {e}"
        return rec

    agent = create_pharmacy_agent(f"eval-{case['id']}", auth, llm=llm)
    called = []
    inputs = []
    texts = []
    outputs = []
    tokens = 0
    t0 = time.perf_counter()
    try:
        async for ev in agent.astream_events(
            {"messages": [HumanMessage(content=case["question"])]},
            config={"configurable": {"session_id": f"eval-{case['id']}"}},
            version="v2",
        ):
            event = ev["event"]
            if event == "on_tool_start":
                called.append(ev["name"])
                inputs.append(ev["data"].get("input") or {})
            elif event == "on_tool_end":
                outputs.append(str(ev["data"].get("output") or "")[:6000])
            elif event == "on_chat_model_stream":
                chunk = ev["data"].get("chunk")
                text = _chunk_text(getattr(chunk, "content", None))
                if text and text.strip():
                    texts.append(text)
                um = getattr(chunk, "usage_metadata", None)
                if isinstance(um, dict):
                    tokens += (um.get("total_tokens")
                               or (um.get("input_tokens") or 0) + (um.get("output_tokens") or 0))
    except Exception as e:  # noqa: BLE001
        rec["error"] = f"agent run: {e}\n{traceback.format_exc(limit=2)}"

    rec["latency_ms"] = round((time.perf_counter() - t0) * 1000, 1)
    rec["tokens"] = tokens or None
    rec["answer"] = "".join(texts)
    rec["called_tools"] = called
    rec["tool_outputs"] = outputs
    
    
    for name, args in zip(called, inputs):
        rec["tool_calls"].append({"name": name, "args": args})
    rec["tool_f1"] = tool_f1(rec["expected_tools"], called)
    
    
    if outputs:
        rec["hallucinated_numbers"] = [
            n for n in hallucinated_numbers(rec["answer"] or "", outputs)
        ][:10]

    if judge and not rec["error"]:
        j = judge_answer(rec["question"], rec["category"], rec["expected_tools"],
                         called, rec["answer"], outputs)
        rec["judge_score"] = j["score"]
        rec["judge_reason"] = j["reason"]
    return rec


async def run_all(args) -> pathlib.Path:
    cases = load_cases()
    if args.limit > 0:
        cases = cases[: args.limit]

    auth = auth_from_env()
    ctx = mock_backend() if args.mode == "mock" else _noop()
    results = []
    async with ctx:
        for i, case in enumerate(cases, 1):
            rec = await run_case(case, args.provider, auth, judge=args.judge)
            results.append(rec)
            status = "ERR" if rec["error"] else f"F1={rec.get('tool_f1')}"
            logger.info("[%d/%d] %s %s -> %s | %sms",
                        i, len(cases), rec["id"], rec["category"], status,
                        rec["latency_ms"] or 0)

    stamp = datetime.now().strftime("%Y%m%d-%H%M%S")
    outdir = pathlib.Path(args.out) / f"{args.mode}_{args.provider}_{stamp}"
    info = write_report(results, outdir)
    runtime = {
        "mode": args.mode, "provider": args.provider, "judge": args.judge,
        "timestamp": stamp,
    }
    (outdir / "runtime.json").write_text(
        json.dumps(runtime, ensure_ascii=False, indent=2), encoding="utf-8")
    return pathlib.Path(info["path"])


def main():
    logging.basicConfig(level=logging.INFO,
                        format="%(asctime)s %(levelname)s %(message)s")
    args = parse_args()
    md = asyncio.run(run_all(args))
    print(f"\n[OK] Báo cáo: {md}")


if __name__ == "__main__":
    main()