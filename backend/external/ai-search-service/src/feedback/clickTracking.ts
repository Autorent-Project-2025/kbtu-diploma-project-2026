import { sql } from "../db/sql";
import { observabilityLogger } from "../observability/logger";

export type ClickRecord = {
  userId: string | null;
  sessionId: string | null;
  prompt: string;
  partnerCarId: number;
  position: number;
};

export async function recordRecommendationClick(click: ClickRecord): Promise<void> {
  try {
    await sql`
      insert into ai_recommendation_clicks (user_id, session_id, prompt, partner_car_id, position)
      values (${click.userId}, ${click.sessionId}, ${click.prompt}, ${click.partnerCarId}, ${click.position})
    `;
    observabilityLogger.info("recommendation_click_recorded", {
      userId: click.userId,
      partnerCarId: click.partnerCarId,
      position: click.position,
    });
  } catch (error) {
    observabilityLogger.warn("recommendation_click_failed", {
      errorMessage: error instanceof Error ? error.message : String(error),
    });
  }
}
