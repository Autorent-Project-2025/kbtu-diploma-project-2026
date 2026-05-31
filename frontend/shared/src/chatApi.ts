import type { AxiosInstance } from "axios";
import type { ChatMessage, Conversation } from "./types/chat";

export function createChatApi(api: AxiosInstance) {
  async function getConversationByContext(
    contextType: string,
    contextId: string,
  ): Promise<Conversation | null> {
    try {
      const { data } = await api.get<Conversation>(
        `/chat/conversations/by-context/${contextType}/${contextId}`,
      );
      return data;
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } })?.response
        ?.status;
      if (status === 404) {
        return null;
      }

      throw err;
    }
  }

  async function getMessages(
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

  async function sendMessage(
    conversationId: string,
    body: string,
    internal = false,
    files?: File[],
  ): Promise<ChatMessage> {
    const formData = new FormData();
    formData.append("body", body);
    formData.append("internal", String(internal));

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

  async function getAttachmentTemporaryLink(
    conversationId: string,
    attachmentId: string,
  ): Promise<string> {
    const { data } = await api.get<{ url: string }>(
      `/chat/conversations/${conversationId}/attachments/${attachmentId}/temporary-link`,
    );
    return data.url;
  }

  return {
    getConversationByContext,
    getMessages,
    sendMessage,
    getAttachmentTemporaryLink,
  };
}
