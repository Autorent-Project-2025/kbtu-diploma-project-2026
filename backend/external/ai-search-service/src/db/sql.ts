import postgres from "postgres";
import { config } from "../config/env";

export const sql = postgres(config.databaseUrl, {
  max: 10,
  idle_timeout: 20,
  connect_timeout: 15,
});

export async function closeSql() {
  await sql.end({ timeout: 5 });
}
