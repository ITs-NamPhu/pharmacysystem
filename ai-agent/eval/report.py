"""Tổng hợp kết quả evaluation -> summary.json + summary.md."""
import json
import statistics
from pathlib import Path

from eval.metrics import precision_over_grounding


def aggregate(results: list) -> dict:
    """Tính trung bình chỉ số toàn cục và theo danh mục."""
    def avg(key, items):
        vals = [r[key] for r in items if r.get(key) is not None]
        return round(statistics.mean(vals), 4) if vals else None

    def summarize(items):
        n = len(items)
        errs = [r for r in items if r.get("error")]
        judged = [r for r in items if r.get("judge_score") is not None]
        scores = [r["judge_score"] for r in judged]
        hall = [r for r in items if r.get("hallucinated_numbers")]
        grounding = [r for r in items if r.get("grounding_precision") is not None]
        return {
            "case_count": n,
            "error_count": len(errs),
            "avg_tool_f1": avg("tool_f1", items),
            "avg_judge_score": (round(statistics.mean(scores), 4) if scores else None),
            "judged_count": len(judged),
            "hallucination_cases": len(hall),
            "avg_grounding_precision": (round(
                statistics.mean(r["grounding_precision"] for r in grounding), 4
            ) if grounding else None),
            "avg_latency_ms": avg("latency_ms", items),
            "total_tokens": sum(r.get("tokens") or 0 for r in items),
        }

    by_category = {}
    for r in results:
        by_category.setdefault(r["category"], []).append(r)

    return {
        "overall": summarize(results),
        "by_category": {cat: summarize(items) for cat, items in sorted(by_category.items())},
    }


def _fmt(v, suffix=""):
    return f"{v:g}{suffix}" if v is not None else "-"


def write_report(results: list, outdir: Path) -> dict:
    outdir = Path(outdir)
    outdir.mkdir(parents=True, exist_ok=True)

    for r in results:
        r["tool_f1"] = round(r.get("tool_f1") or 0.0, 4)
        if r.get("answer") or r.get("tool_outputs"):
            r["grounding_precision"] = round(
                precision_over_grounding(r.get("answer") or "",
                                         r.get("tool_outputs") or []), 4)

    (outdir / "results.json").write_text(
        json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8")

    summary = aggregate(results)
    (outdir / "summary.json").write_text(
        json.dumps(summary, ensure_ascii=False, indent=2), encoding="utf-8")

    lines = ["# Báo cáo Evaluation AI Agent\n"]
    ow = summary["overall"]
    lines.append(f"- Case: **{ow['case_count']}** | Lỗi chạy: **{ow['error_count']}**")
    lines.append(f"- Tool F1 trung bình: **{_fmt(ow['avg_tool_f1'])}**")
    lines.append(f"- Judge score trung bình: **{_fmt(ow['avg_judge_score'])}**"
                 f" (chấm {ow['judged_count']} case)")
    lines.append(f"- Case nghi hallucination (số lạ): **{ow['hallucination_cases']}**")
    lines.append(f"- Grounding precision: **{_fmt(ow['avg_grounding_precision'])}**")
    lines.append(f"- Latency trung bình: **{_fmt(ow['avg_latency_ms'], ' ms')}**")
    lines.append(f"- Tổng token: **{_fmt(ow['total_tokens'])}**\n")

    lines.append("| Danh mục | Case | F1 | Judge | Hallu | Grounding | Latency(ms) | Tokens |")
    lines.append("|---|---|---|---|---|---|---|---|")
    for cat, s in summary["by_category"].items():
        lines.append(
            f"| {cat} | {s['case_count']} | {_fmt(s['avg_tool_f1'])} | {_fmt(s['avg_judge_score'])} "
            f"| {s['hallucination_cases']} | {_fmt(s['avg_grounding_precision'])} "
            f"| {_fmt(s['avg_latency_ms'])} | {_fmt(s['total_tokens'])} |")
    lines.append("")

    lines.append("\n## Chi tiết case\n")
    for r in results:
        status = "ERR" if r.get("error") else "ok"
        lines.append(f"### [{status}] `{r['id']}` · {r['category']} · F1={_fmt(r['tool_f1'])} "
                     f"· Judge={_fmt(r.get('judge_score'))}")
        lines.append(f"**Q:** {r['question']}")
        lines.append(f"**Tool dùng:** {', '.join(r.get('called_tools') or []) or '(không gọi)'} "
                     f"(dự kiến: {', '.join(r.get('expected_tools') or []) or '(không cần)'})")
        if r.get("tool_calls"):
            lines.append(f"**Tool calls:** {r['tool_calls']}")
        if r.get("error"):
            lines.append(f"**Lỗi:** {r['error']}")
        else:
            lines.append(f"**A:** {r.get('answer') or '(trống)'}")
        if r.get("hallucinated_numbers"):
            lines.append(f"*⚠️ Số không có trong tool output: {r['hallucinated_numbers']}*")
        if r.get("judge_reason"):
            lines.append(f"*Judge: {r['judge_reason']}*")
        lines.append("")

    md_path = outdir / "summary.md"
    md_path.write_text("\n".join(lines), encoding="utf-8")
    return {"path": str(md_path), "summary": summary}