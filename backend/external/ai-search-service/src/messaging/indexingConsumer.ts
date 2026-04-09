import amqp, { type ChannelModel } from "amqplib";
import { config } from "../config/env";
import { observabilityLogger } from "../observability/logger";
import { deleteDocumentByPartnerCarId, reindexPartnerCar } from "../indexing/searchIndexer";

const queueName = "ai-search-service.indexing";
const upsertRoutingKey = "car.search.partner-car-upserted";
const deleteRoutingKey = "car.search.partner-car-deleted";

type IntegrationMessage<TPayload> = {
  eventId?: string;
  routingKey?: string;
  occurredAtUtc?: string;
  payload?: TPayload;
};

type PartnerCarSearchDocumentChanged = {
  partnerCarId: number;
  changeType: string;
};

export async function startIndexingConsumer(): Promise<ChannelModel | null> {
  if (!config.rabbitMqUrl) {
    observabilityLogger.warn("rabbitmq_consumer_disabled", {
      reason: "RABBITMQ_URL is not configured",
    });
    return null;
  }

  const connection = await amqp.connect(config.rabbitMqUrl);
  const channel = await connection.createChannel();

  await channel.assertExchange(config.rabbitMqExchange, "topic", { durable: true });
  await channel.assertQueue(queueName, { durable: true });
  await channel.bindQueue(queueName, config.rabbitMqExchange, upsertRoutingKey);
  await channel.bindQueue(queueName, config.rabbitMqExchange, deleteRoutingKey);

  await channel.consume(queueName, async (message) => {
    if (!message) {
      return;
    }

    try {
      const envelope = JSON.parse(message.content.toString("utf8")) as IntegrationMessage<PartnerCarSearchDocumentChanged>;
      const payload = envelope.payload;
      if (!payload || typeof payload.partnerCarId !== "number") {
        throw new Error("PartnerCarId is missing in indexing event payload.");
      }

      if (message.fields.routingKey === deleteRoutingKey) {
        await deleteDocumentByPartnerCarId(payload.partnerCarId);
      } else {
        await reindexPartnerCar(payload.partnerCarId);
      }

      channel.ack(message);
    } catch (error) {
      observabilityLogger.error("rabbitmq_indexing_event_failed", error, {
        routingKey: message.fields.routingKey,
      });
      channel.nack(message, false, false);
    }
  });

  observabilityLogger.info("rabbitmq_indexing_consumer_started", {
    queueName,
    upsertRoutingKey,
    deleteRoutingKey,
  });

  return connection;
}
