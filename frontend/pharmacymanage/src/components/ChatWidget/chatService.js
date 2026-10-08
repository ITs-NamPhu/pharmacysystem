import { jwtDecode } from 'jwt-decode';
import instance from '../../utils/axiosCustomize';

const AI_AGENT_URL = process.env.REACT_APP_AI_URL || 'http://localhost:8000';
const MESSAGE_LIMIT = 20;

// Token sắp hết hạn chưa 1 phút (hoặc đã hết hạn)? -> cần refresh trước
export const isTokenExpiringSoon = (accessToken, thresholdSec = 60) => {
    if (!accessToken) return false;
    try {
        const { exp } = jwtDecode(accessToken); // exp tính bằng giây (Unix)
        if (!exp) return false;
        return exp * 1000 - Date.now() < thresholdSec * 1000;
    } catch {
        return false;
    }
};

// Chuẩn hóa tin nhắn từ .NET (ChatMessageDto) -> shape ChatMessage component
export const normalizeApiMessage = (m) => ({
    id: m.messageId,
    sender: m.role === 'user' ? 'user' : 'ai',
    content: m.content || '',
    timestamp: m.createdAt,
    done: true,
});

export const normalizeConversation = (c) => ({
    id: c.conversationId,
    title: c.title || 'Cuộc trò chuyện',
    preview: c.lastMessagePreview || '',
    updatedAt: c.updatedAt || c.lastMessageAt,
});

export const sendMessage = async (message, token, branchId, conversationId, sessionId) => {
    const response = await fetch(`${AI_AGENT_URL}/ai/chat`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': token,
            'X-Branch-Id': branchId || '',
        },
        body: JSON.stringify({
            message,
            session_id: sessionId || (conversationId ? String(conversationId) : 'default'),
            conversation_id: conversationId || null,
        }),
    });

    if (!response.ok) {
        throw new Error(`AI Agent error: ${response.status}`);
    }

    return response;
};

export const clearChatHistory = async (token, sessionId = 'default') => {
    const response = await fetch(`${AI_AGENT_URL}/api/ai/chat/clear?session_id=${sessionId}`, {
        method: 'POST',
        headers: {
            'Authorization': token,
        },
    });
    return response.json();
};

// Danh sách hội thoại của user (gọi thẳng .NET, instance tự đính Bearer token + X-Branch-Id)
export const getConversations = async () => {
    const response = await instance.get('api/chat/conversations', {
        params: { limit: 50 },
    });
    if (response?.ec !== 0) {
        throw new Error(response?.em || 'Lỗi tải danh sách hội thoại');
    }
    return (response?.dt || []).map(normalizeConversation);
};

// Tin nhắn của một hội thoại (cursor pagination theo beforeId)
export const getMessages = async (conversationId, beforeId) => {
    const params = { limit: MESSAGE_LIMIT };
    if (beforeId) params.beforeId = beforeId;

    const response = await instance.get(`api/chat/conversations/${conversationId}/messages`, {
        params,
    });
    if (response?.ec !== 0) {
        throw new Error(response?.em || 'Lỗi tải tin nhắn');
    }
    return (response?.dt || []).map(normalizeApiMessage);
};

export const parseSSEStream = (reader, decoder, onChunk) => {
    let buffer = '';

    const processStream = async () => {
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            const lines = buffer.split('\n');
            buffer = lines.pop() || '';

            for (const line of lines) {
                if (line.startsWith('data: ')) {
                    try {
                        const data = JSON.parse(line.slice(6));
                        onChunk(data);
                    } catch (e) {
                        // ignore parse errors for incomplete chunks
                    }
                }
            }
        }
    };

    return processStream();
};