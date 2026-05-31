import { createMailer } from "./mailer/mailer.ts";
import { observabilityLogger } from "./observability/logger.ts";
import { startRabbitConsumer } from "./rabbitmq/consumer.ts";
import { createEmailHttpServer } from "./http/server.ts";
import { parsePort } from "./http/httpUtils.ts";

async function main() {
  const mailer = createMailer();
  const port = parsePort(process.env.PORT);

  if (process.env.SMTP_VERIFY_ON_STARTUP === "true") {
    await mailer.verify();
    observabilityLogger.info("smtp_connection_verified", {
      host: process.env.SMTP_HOST ?? null,
      port: process.env.SMTP_PORT ?? null,
    });
  }

  await startRabbitConsumer(mailer);

  const server = createEmailHttpServer(mailer);
  server.listen(port, () => {
    observabilityLogger.info("server_started", { port });
  });
}

main().catch((error) => {
  observabilityLogger.error("service_startup_failed", error);
  process.exit(1);
});
