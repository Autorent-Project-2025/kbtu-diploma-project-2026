export interface Conversation {
  id: string;
  contextType: string;
  contextId: string;
  sourceService: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  closedAt: string | null;
  participants: Participant[];
}

export interface Participant {
  userId: string;
  actorType: string;
  role: string;
  canRead: boolean;
  canWrite: boolean;
  canSendInternal: boolean;
  joinedAt: string;
  leftAt: string | null;
  lastReadMessageId: string | null;
  lastReadAt: string | null;
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderUserId: string;
  senderActorType: string;
  messageType: string;
  visibility: string;
  body: string;
  createdAt: string;
  attachments: ChatAttachment[];
}

export interface ChatAttachment {
  id: string;
  fileName: string;
  originalFileName: string;
  mimeType: string;
  createdAt: string;
}
