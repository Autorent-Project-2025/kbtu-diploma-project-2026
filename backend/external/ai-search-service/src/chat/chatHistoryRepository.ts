import { sql } from "../db/sql";
import { observabilityLogger } from "../observability/logger";
import { AiChatHistoryResponse } from "../types";
import { normalizeChatMessages } from "./chatMessageNormalization";

type RawChatHistoryRow = {
  userId: string;
  messages: unknown;
};

export async function getChatHistory(userId: string): Promise<AiChatHistoryResponse> {
  const rows = await sql<RawChatHistoryRow[]>`
    select
      user_id as "userId",
      messages
    from ai_chat_histories
    where user_id = ${userId}
    limit 1
  `;

  const normalizedMessages = normalizeChatMessages(rows[0]?.messages ?? []);

  observabilityLogger.info("chat_history_loaded", {
    userId,
    messagesCount: normalizedMessages.length,
  });

  return {
    messages: normalizedMessages,
  };
}

export async function saveChatHistory(
  userId: string,
  messages: unknown,
): Promise<AiChatHistoryResponse> {
  const normalizedMessages = normalizeChatMessages(messages);

  await sql`
    insert into ai_chat_histories (
      user_id,
      messages,
      updated_at
    )
    values (
      ${userId},
      ${sql.json(normalizedMessages)},
      now()
    )
    on conflict (user_id) do update
    set
      messages = excluded.messages,
      updated_at = now()
  `;

  observabilityLogger.info("chat_history_saved", {
    userId,
    messagesCount: normalizedMessages.length,
  });

  return {
    messages: normalizedMessages,
  };
}
