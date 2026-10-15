import asyncio
import json
import logging
from typing import AsyncGenerator, Optional

import httpx
from fastapi import APIRouter, Depends
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage, AIMessage

from app.api.deps import get_auth_context
from app.models.chat import ChatRequest, AuthContext
from app.agents.supervisor import create_supervisor_graph
from app.agents.router import ROUTER_TAG
from app.agents.pharmacy_agent import build_agent_callbacks, build_langfuse_metadata
from app.agents.memory import memory_manager
from app.config.settings import settings
from app.exceptions import TokenExpiredException
from app.services.backend_client import backend_client

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/ai", tags=["AI Chat"])


def _normalize_token(token: str) -> str:
    """Thêm prefix 'Bearer ' cho token nếu React/FastAPI truyền thiếu."""
    if token and not token.lower().startswith("bearer "):
        return f"Bearer {token}"
    return token


def _mark_expired_on_401(resp: httpx.Response):
    """Nếu .NET trả EC=-999 (access token hết hạn) thì bật cờ báo cho stream."""
    if resp.status_code == 401:
        try:
            body = resp.json()
        except ValueError:
            body = {}
        if (body.get("ec") or body.get("EC")) == -999:
            backend_client.token_expired = True


async def load_history_from_dotnet(conversation_id: int, auth: AuthContext) -> list:
    """Kéo lịch sử chat từ .NET để nạp vào ngữ cảnh (Memory) của agent."""
    messages = []
    try:
        async with httpx.AsyncClient(timeout=20.0) as client:
            resp = await client.get(
                f"{settings.BACKEND_URL}/api/chat/conversations/{conversation_id}/messages",
                params={"limit": settings.MAX_CHAT_HISTORY},
                headers={"Authorization": _normalize_token(auth.token)},
            )
            _mark_expired_on_401(resp)
            resp.raise_for_status()
            body = resp.json()
            dt = body.get("dt") or body.get("DT")
            if isinstance(dt, list):
                for item in dt:
                    role = (item.get("role") or "").lower()
                    content = item.get("content") or ""
                    if role == "user":
                        messages.append(HumanMessage(content=content))
                    elif role in ("ai", "assistant"):
                        messages.append(AIMessage(content=content))
    except Exception as e:
        logger.error(f"Lỗi tải lịch sử chat từ .NET: {str(e)}")
    return messages


async def create_conversation_in_dotnet(first_message: str, auth: AuthContext) -> Optional[int]:
    """Tạo conversation mới trên .NET trước khi stream để có conversationId truyền về UI."""
    try:
        async with httpx.AsyncClient(timeout=15.0) as client:
            resp = await client.post(
                f"{settings.BACKEND_URL}/api/chat/conversations",
                json={"firstMessage": first_message},
                headers={"Authorization": _normalize_token(auth.token)},
            )
            _mark_expired_on_401(resp)
            resp.raise_for_status()
            body = resp.json()
            dt = body.get("dt") or body.get("DT")
            return dt.get("conversationId") if isinstance(dt, dict) else None
    except Exception as e:
        logger.error(f"Lỗi tạo conversation trên .NET: {str(e)}")
        return None


async def save_history_to_dotnet(conversation_id: int, user_message: str, ai_message: str, auth: AuthContext):
    """Background task: gửi cặp tin nhắn (User + AI) về .NET sau khi stream xong."""
    try:
        payload = {
            "conversationId": conversation_id,
            "userMessage": user_message,
            "aiMessage": ai_message,
        }
        headers = {
            "Authorization": _normalize_token(auth.token),
        }
        if auth.branch_id:
            headers["X-Branch-Id"] = auth.branch_id

        async with httpx.AsyncClient(timeout=20.0) as client:
            await client.post(
                f"{settings.BACKEND_URL}/api/chat/messages",
                json=payload,
                headers=headers,
            )
    except Exception as e:
        logger.error(f"Lỗi khi lưu lịch sử chat vào .NET: {str(e)}")


@router.post("/chat")
async def chat(
    request: ChatRequest,
    auth: AuthContext = Depends(get_auth_context),
):
    conversation_id = request.conversation_id
    history_messages: list = []

    if conversation_id:
        # Tiếp tục hội thoại cũ: nạp lịch sử từ DB làm ngữ cảnh
        history_messages = await load_history_from_dotnet(conversation_id, auth)
    else:
        # Hội thoại mới: tạo conversation trước để có conversationId truyền về UI
        conversation_id = await create_conversation_in_dotnet(request.message, auth)

    session_id = str(conversation_id) if conversation_id else request.session_id
    agent = create_supervisor_graph(session_id, auth)
    config = {"configurable": {"session_id": session_id}}
    callbacks = build_agent_callbacks()
    if callbacks:
        config["callbacks"] = callbacks
        langfuse_metadata = build_langfuse_metadata(session_id, auth)
        if langfuse_metadata:
            config["metadata"] = {**config.get("metadata", {}), **langfuse_metadata}

    async def stream_response() -> AsyncGenerator[str, None]:
        # Biến tích lũy toàn bộ câu trả lời của AI
        full_ai_response = ""
        backend_client.token_expired = False  # reset cờ cho request hiện tại

        try:
            # Ngữ cảnh cũ (nếu có) + câu hỏi mới để LLM đọc được cả lịch sử
            input_messages = [*history_messages, ("user", request.message)]

            
            async for event in agent.astream_events(
                {"messages": input_messages},
                # payload,
                config=config,
                version="v2",
            ):
                # Token chết giữa chừng -> dừng luồng ngay, không để AI trả nội dung lỗi
                if backend_client.token_expired:
                    break

                kind = event.get("event", "")

                # Bỏ qua event của router chỉ quyết định đi đâu,
                # không phải nội dung trả lời cho người dùng.
                if ROUTER_TAG in (event.get("tags") or []):
                    continue

                if kind == "on_chat_model_stream":
                    chunk = event.get("data", {}).get("chunk")
                    if chunk and hasattr(chunk, "content") and chunk.content:
                        # Xử lý trích xuất văn bản an toàn
                        text_content = ""
                        if isinstance(chunk.content, str):
                            text_content = chunk.content
                        elif isinstance(chunk.content, list):
                            # Lọc lấy text nếu content là một mảng các block dữ liệu
                            text_content = "".join([
                                item.get("text", "") if isinstance(item, dict) else str(item)
                                for item in chunk.content
                            ])
                        
                        # Chỉ cộng dồn và yield về UI nếu có nội dung chữ
                        if text_content:
                            full_ai_response += text_content
                            yield f"data: {json.dumps({'type': 'text', 'content': text_content}, ensure_ascii=False)}\n\n"
                            
                elif kind == "on_tool_start":
                    tool_name = event.get("name", "unknown")
                    tool_input = event.get("data", {}).get("input", {})
                    yield f"data: {json.dumps({'type': 'tool_start', 'tool': tool_name, 'input': str(tool_input)[:200]}, ensure_ascii=False)}\n\n"

                elif kind == "on_tool_end":
                    tool_name = event.get("name", "unknown")
                    yield f"data: {json.dumps({'type': 'tool_end', 'tool': tool_name}, ensure_ascii=False)}\n\n"

        except TokenExpiredException:
            # Exception ngắt trực tiếp lên chat.py -> vẫn báo auth_error
            backend_client.token_expired = True
            logger.warning("Token expired detected during agent stream")
        except Exception as e:
            logger.exception("Agent stream error")
            yield f"data: {json.dumps({'type': 'error', 'content': f'Đã xảy ra lỗi: {str(e)}'}, ensure_ascii=False)}\n\n"
            return

        # Bắn tín hiệu riêng cho React biết do token hết hạn (tự refresh + retry)
        if backend_client.token_expired:
            backend_client.token_expired = False
            yield f"data: {json.dumps({'type': 'auth_error', 'content': 'Token expired'}, ensure_ascii=False)}\n\n"
            return

        # KHI STREAM KẾT THÚC: chạy ngầm lưu cặp tin nhắn về .NET (không delay UI)
        if conversation_id:
            asyncio.create_task(
                save_history_to_dotnet(
                    conversation_id=conversation_id,
                    user_message=request.message,
                    ai_message=full_ai_response,
                    auth=auth,
                )
            )

        # Báo cho UI biết đã xong + cung cấp conversationId
        yield f"data: {json.dumps({'type': 'done', 'conversationId': conversation_id}, ensure_ascii=False)}\n\n"

    return StreamingResponse(
        stream_response(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "X-Accel-Buffering": "no",
            "Connection": "keep-alive",
        },
    )


@router.post("/chat/clear")
async def clear_chat(
    session_id: str = "default",
    auth: AuthContext = Depends(get_auth_context),
):
    memory_manager.clear(session_id)
    return {"status": "ok", "message": f"Da xoa lich su chat session {session_id}"}
