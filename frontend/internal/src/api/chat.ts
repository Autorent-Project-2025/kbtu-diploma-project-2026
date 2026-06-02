import { createChatApi } from "@shared/chatApi";
import api from "./axios";

const chatApi = createChatApi(api);

export const getConversationByContext = chatApi.getConversationByContext;
export const getMessages = chatApi.getMessages;
export const sendMessage = chatApi.sendMessage;
export const getAttachmentTemporaryLink = chatApi.getAttachmentTemporaryLink;
