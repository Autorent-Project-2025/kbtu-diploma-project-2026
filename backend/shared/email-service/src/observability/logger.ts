import { createWriteStream } from "node:fs";
import { mkdirSync } from "node:fs";
import { dirname } from "node:path";

type LogLevel = "info" | "warn" | "error";
type LogPayload = Record<string, unknown>;

const SERVICE_NAME = "email-service";
const observabilityLogPath = process.env.OBSERVABILITY_LOG_PATH?.trim();
const observabilityLogStream = observabilityLogPath
  ? (() => {
      mkdirSync(dirname(observabilityLogPath), { recursive: true });
      return createWriteStream(observabilityLogPath, { flags: "a" });
    })()
  : null;

function buildErrorFields(error: unknown): LogPayload {
  if (error instanceof Error) {
    return {
      errorName: error.name,
      errorMessage: error.message,
      errorStack: error.stack ?? null,
    };
  }

  return {
    errorMessage: typeof error === "string" ? error : String(error),
  };
}

function write(level: LogLevel, event: string, payload: LogPayload = {}) {
  const entry = {
    timestamp: new Date().toISOString(),
    level,
    service: SERVICE_NAME,
    event,
    ...payload,
  };

  const line = JSON.stringify(entry);

  if (level === "error") {
    console.error(line);
  } else if (level === "warn") {
    console.warn(line);
  } else {
    console.log(line);
  }

  observabilityLogStream?.write(`${line}\n`);
}

process.once("exit", () => {
  observabilityLogStream?.end();
});

export const observabilityLogger = {
  info(event: string, payload: LogPayload = {}) {
    write("info", event, payload);
  },

  warn(event: string, payload: LogPayload = {}) {
    write("warn", event, payload);
  },

  error(event: string, error: unknown, payload: LogPayload = {}) {
    write("error", event, {
      ...payload,
      ...buildErrorFields(error),
    });
  },
};
