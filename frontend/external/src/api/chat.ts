import { createChatApi } from "@shared/chatApi";
import api from "./axios";

const chatApi = createChatApi(api);

export const getConversationByContext = chatApi.getConversationByContext;
export const getMessages = chatApi.getMessages;
export const getAttachmentTemporaryLink = chatApi.getAttachmentTemporaryLink;

export function sendMessage(
  conversationId: string,
  body: string,
  files?: File[],
) {
  return chatApi.sendMessage(conversationId, body, false, files);
}
