import api from "./axios";
import type { Conversation, ChatMessage } from "../types/Chat";

export async function getConversationByContext(
  contextType: string,
  contextId: string,
): Promise<Conversation | null> {
  try {
    const { data } = await api.get<Conversation>(
      `/chat/conversations/by-context/${contextType}/${contextId}`,
    );
    return data;
  } catch (err: any) {
    if (err?.response?.status === 404) {
      return null;
    }
    throw err;
  }
}

export async function getMessages(
  conversationId: string,
  before?: string,
  limit = 50,
): Promise<ChatMessage[]> {
  const params: Record<string, string | number> = { limit };
  if (before) params.before = before;
  const { data } = await api.get<ChatMessage[]>(
    `/chat/conversations/${conversationId}/messages`,
    { params },
  );
  return data;
}

export async function sendMessage(
  conversationId: string,
  body: string,
  files?: File[],
): Promise<ChatMessage> {
  const formData = new FormData();
  formData.append("body", body);
  formData.append("internal", "false");
  if (files) {
    for (const file of files) {
      formData.append("files", file);
    }
  }
  const { data } = await api.post<ChatMessage>(
    `/chat/conversations/${conversationId}/messages`,
    formData,
  );
  return data;
}

export async function getAttachmentTemporaryLink(
  conversationId: string,
  attachmentId: string,
): Promise<string> {
  const { data } = await api.get<{ url: string }>(
    `/chat/conversations/${conversationId}/attachments/${attachmentId}/temporary-link`,
  );
  return data.url;
}
