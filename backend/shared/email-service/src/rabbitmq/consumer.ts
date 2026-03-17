import amqplib from "amqplib";
import { createMailer } from "../mailer/mailer.ts";
import { observabilityLogger } from "../observability/logger.ts";
import {
  approvedTemplate,
  partnerCarApprovedTemplate,
  partnerCarRejectedTemplate,
  partnerApprovedTemplate,
  partnerRejectedTemplate,
  rejectedTemplate,
} from "../mailer/templates/index.ts";

type Mailer = ReturnType<typeof createMailer>;

type IntegrationMessage<TPayload> = {
  eventId: string;
  routingKey: string;
  occurredAtUtc: string;
  payload: TPayload;
};

type ClientApprovedEmailRequested = {
  ticketId: string;
  to: string;
  fullName: string;
  loginEmail: string;
  setPasswordUrl: string;
};

type ClientRejectedEmailRequested = {
  ticketId: string;
  to: string;
  fullName: string;
  reason?: string;
};

type PartnerApprovedEmailRequested = ClientApprovedEmailRequested;
type PartnerRejectedEmailRequested = ClientRejectedEmailRequested;

type PartnerCarApprovedEmailRequested = {
  ticketId: string;
  to: string;
  fullName: string;
  carBrand: string;
  carModel: string;
  licensePlate: string;
};

type PartnerCarRejectedEmailRequested = PartnerCarApprovedEmailRequested & {
  reason?: string;
};

const EXCHANGE_NAME = process.env.RABBITMQ_EXCHANGE?.trim() || "autorent.events";
const QUEUE_NAME = "email-service.notifications";
const RECONNECT_DELAY_MS = 5000;
const DEDUP_TTL_MS = parsePositiveInt(process.env.EMAIL_EVENT_DEDUP_TTL_MS, 15 * 60 * 1000);
const ROUTING_KEYS = [
  "ticket.email.client-approved",
  "ticket.email.client-rejected",
  "ticket.email.partner-approved",
  "ticket.email.partner-rejected",
  "ticket.email.partner-car-approved",
  "ticket.email.partner-car-rejected",
];

function parsePositiveInt(value: string | undefined, fallback: number): number {
  if (!value) return fallback;

  const parsed = Number(value);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error(`Invalid positive integer value: "${value}"`);
  }

  return parsed;
}

function getRabbitMqUrl(): string {
  return process.env.RABBITMQ_URL?.trim() || "amqp://guest:guest@rabbitmq:5672";
}

function isObject(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function requireString(value: unknown, field: string): string {
  if (typeof value !== "string" || value.trim() === "") {
    throw new Error(`${field} is required`);
  }

  return value.trim();
}

function optionalString(value: unknown): string | undefined {
  if (value === undefined || value === null || value === "") {
    return undefined;
  }

  if (typeof value !== "string") {
    throw new Error("Optional field must be a string");
  }

  return value.trim();
}

function parseIntegrationMessage<TPayload>(content: Buffer): IntegrationMessage<TPayload> {
  const parsed = JSON.parse(content.toString("utf8")) as unknown;
  if (!isObject(parsed)) {
    throw new Error("Integration message must be an object");
  }

  return {
    eventId: requireString(parsed.eventId, "eventId"),
    routingKey: requireString(parsed.routingKey, "routingKey"),
    occurredAtUtc: requireString(parsed.occurredAtUtc, "occurredAtUtc"),
    payload: parsed.payload as TPayload,
  };
}

function cleanupProcessedEvents(processedEvents: Map<string, number>) {
  const now = Date.now();
  for (const [eventId, expiresAt] of processedEvents.entries()) {
    if (expiresAt <= now) {
      processedEvents.delete(eventId);
    }
  }
}

async function sendTemplateEmail(mailer: Mailer, routingKey: string, payload: unknown) {
  if (!isObject(payload)) {
    throw new Error("Payload must be an object");
  }

  switch (routingKey) {
    case "ticket.email.client-approved": {
      const typedPayload = payload as ClientApprovedEmailRequested;
      const template = approvedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        loginEmail: requireString(typedPayload.loginEmail, "payload.loginEmail"),
        setPasswordUrl: requireString(typedPayload.setPasswordUrl, "payload.setPasswordUrl"),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    case "ticket.email.client-rejected": {
      const typedPayload = payload as ClientRejectedEmailRequested;
      const template = rejectedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        reason: optionalString(typedPayload.reason),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    case "ticket.email.partner-approved": {
      const typedPayload = payload as PartnerApprovedEmailRequested;
      const template = partnerApprovedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        loginEmail: requireString(typedPayload.loginEmail, "payload.loginEmail"),
        setPasswordUrl: requireString(typedPayload.setPasswordUrl, "payload.setPasswordUrl"),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    case "ticket.email.partner-rejected": {
      const typedPayload = payload as PartnerRejectedEmailRequested;
      const template = partnerRejectedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        reason: optionalString(typedPayload.reason),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    case "ticket.email.partner-car-approved": {
      const typedPayload = payload as PartnerCarApprovedEmailRequested;
      const template = partnerCarApprovedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        carBrand: requireString(typedPayload.carBrand, "payload.carBrand"),
        carModel: requireString(typedPayload.carModel, "payload.carModel"),
        licensePlate: requireString(typedPayload.licensePlate, "payload.licensePlate"),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    case "ticket.email.partner-car-rejected": {
      const typedPayload = payload as PartnerCarRejectedEmailRequested;
      const template = partnerCarRejectedTemplate({
        fullName: requireString(typedPayload.fullName, "payload.fullName"),
        carBrand: requireString(typedPayload.carBrand, "payload.carBrand"),
        carModel: requireString(typedPayload.carModel, "payload.carModel"),
        licensePlate: requireString(typedPayload.licensePlate, "payload.licensePlate"),
        reason: optionalString(typedPayload.reason),
      });

      await mailer.sendMail({
        to: requireString(typedPayload.to, "payload.to"),
        subject: template.subject,
        text: template.text,
        html: template.html,
      });
      return;
    }

    default:
      throw new Error(`Unsupported email routing key: ${routingKey}`);
  }
}

export async function startRabbitConsumer(mailer: Mailer): Promise<void> {
  const processedEvents = new Map<string, number>();

  void (async () => {
    while (true) {
      try {
        const connection = await amqplib.connect(getRabbitMqUrl());
        const channel = await connection.createChannel();

        await channel.assertExchange(EXCHANGE_NAME, "topic", { durable: true });
        await channel.assertQueue(QUEUE_NAME, { durable: true });
        await Promise.all(
          ROUTING_KEYS.map((routingKey) =>
            channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, routingKey),
          ),
        );

        await channel.prefetch(1);

        observabilityLogger.info("rabbitmq_consumer_connected", {
          exchange: EXCHANGE_NAME,
          queue: QUEUE_NAME,
        });

        await channel.consume(
          QUEUE_NAME,
          async (message) => {
            if (!message) {
              return;
            }

            try {
              cleanupProcessedEvents(processedEvents);
              const integrationMessage = parseIntegrationMessage<unknown>(message.content);
              const duplicateExpiresAt = processedEvents.get(integrationMessage.eventId);

              if (duplicateExpiresAt && duplicateExpiresAt > Date.now()) {
                observabilityLogger.warn("rabbitmq_message_duplicate_skipped", {
                  eventId: integrationMessage.eventId,
                  routingKey: integrationMessage.routingKey,
                });
                channel.ack(message);
                return;
              }

              observabilityLogger.info("rabbitmq_message_received", {
                eventId: integrationMessage.eventId,
                routingKey: integrationMessage.routingKey,
              });

              await sendTemplateEmail(mailer, integrationMessage.routingKey, integrationMessage.payload);
              processedEvents.set(integrationMessage.eventId, Date.now() + DEDUP_TTL_MS);
              observabilityLogger.info("rabbitmq_message_processed", {
                eventId: integrationMessage.eventId,
                routingKey: integrationMessage.routingKey,
              });
              channel.ack(message);
            } catch (error) {
              observabilityLogger.error("rabbitmq_message_processing_failed", error, {
                routingKey: message.fields.routingKey || null,
                deliveryTag: message.fields.deliveryTag,
              });
              channel.nack(message, false, true);
              await new Promise((resolve) => setTimeout(resolve, RECONNECT_DELAY_MS));
            }
          },
          { noAck: false },
        );

        await new Promise<void>((resolve, reject) => {
          connection.once("close", () => resolve());
          connection.once("error", (error) => reject(error));
        });
      } catch (error) {
        observabilityLogger.error("rabbitmq_consumer_disconnected", error, {
          exchange: EXCHANGE_NAME,
          queue: QUEUE_NAME,
        });
        await new Promise((resolve) => setTimeout(resolve, RECONNECT_DELAY_MS));
      }
    }
  })();
}
