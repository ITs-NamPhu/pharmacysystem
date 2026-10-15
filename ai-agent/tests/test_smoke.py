import httpx
import pytest
import respx
from fastapi.testclient import TestClient
from langchain_core.messages import AIMessage, HumanMessage

from app.api import chat as chat_module
from app.config.settings import settings
from main import app
from tests.conftest import make_fake_llm, make_router_llm

client = TestClient(app)


def test_health_endpoint():
    r = client.get("/health")
    assert r.status_code == 200
    assert r.json()["status"] == "healthy"


def test_chat_requires_authorization():
    r = client.post("/ai/chat", json={"message": "xin chao"})
    assert r.status_code == 401


def test_clear_requires_authorization():
    # /chat/clear cũng bắt buộc Authorization (get_auth_context)
    r = client.post("/ai/chat/clear", params={"session_id": "x"})
    assert r.status_code == 401


@pytest.fixture
def fake_agent(monkeypatch):
    """Thay create_supervisor_graph trong chat.py bằng graph dùng LLM giả.

    Router luôn chuyển về chat_agent để smoke test chỉ tập trung vào luồng SSE.
    """
    real = chat_module.create_supervisor_graph
    llm = make_fake_llm([AIMessage(content="Cảm ơn bạn!")] * 6)

    def wrapped(session_id, auth):
        return real(
            session_id,
            auth,
            llm=llm,
            router_llm=make_router_llm("chat_agent"),
        )

    monkeypatch.setattr(chat_module, "create_supervisor_graph", wrapped)
    return wrapped


def test_chat_new_conversation_streams_done(fake_agent):
    with respx.mock() as router:
        router.post(f"{settings.BACKEND_URL}/api/chat/conversations").mock(
            return_value=httpx.Response(200, json={"dt": {"conversationId": 42}})
        )
        router.post(f"{settings.BACKEND_URL}/api/chat/messages").mock(
            return_value=httpx.Response(200, json={"dt": None})
        )

        r = client.post(
            "/ai/chat",
            json={"message": "xin chao"},
            headers={"Authorization": "Bearer test-token"},
        )
    assert r.status_code == 200
    body = r.text
    assert '"type": "done"' in body
    assert '"conversationId": 42' in body
    # Nội dung trả lời bị stream từng token -> kiểm tra mảnh đầu tiên + event text
    assert '"type": "text"' in body
    assert "Cảm" in body


def test_chat_continuing_conversation_loads_history(fake_agent):
    with respx.mock() as router:
        router.get(
            f"{settings.BACKEND_URL}/api/chat/conversations/5/messages"
        ).mock(
            return_value=httpx.Response(
                200,
                json={"dt": [{"role": "user", "content": "trước đó hỏi gì?"}]},
            )
        )
        router.post(f"{settings.BACKEND_URL}/api/chat/messages").mock(
            return_value=httpx.Response(200, json={"dt": None})
        )

        r = client.post(
            "/ai/chat",
            json={"message": "trả lời tiếp", "conversation_id": 5},
            headers={"Authorization": "Bearer test-token", "X-Branch-Id": "3"},
        )
    assert r.status_code == 200
    assert '"type": "done"' in r.text
    assert '"conversationId": 5' in r.text


def test_chat_backend_create_failure_still_answers(fake_agent):
    with respx.mock() as router:
        router.post(f"{settings.BACKEND_URL}/api/chat/conversations").mock(
            return_value=httpx.Response(500, text="boom")
        )
        r = client.post(
            "/ai/chat",
            json={"message": "xin chao"},
            headers={"Authorization": "Bearer test-token"},
        )
    assert r.status_code == 200
    assert '"type": "done"' in r.text
