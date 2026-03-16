import { diag, DiagConsoleLogger, DiagLogLevel } from "@opentelemetry/api";
import { getNodeAutoInstrumentations } from "@opentelemetry/auto-instrumentations-node";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { NodeSDK } from "@opentelemetry/sdk-node";

const tracesEndpoint = process.env.OTEL_EXPORTER_OTLP_TRACES_ENDPOINT?.trim();
const serviceName = process.env.OTEL_SERVICE_NAME?.trim() || "api-gateway";

if (process.env.OTEL_DIAGNOSTICS_ENABLED === "true") {
  diag.setLogger(new DiagConsoleLogger(), DiagLogLevel.INFO);
}

if (!tracesEndpoint) {
  console.log("OpenTelemetry tracing is disabled for api-gateway because OTEL_EXPORTER_OTLP_TRACES_ENDPOINT is not set.");
} else {
  const sdk = new NodeSDK({
    serviceName,
    traceExporter: new OTLPTraceExporter({ url: tracesEndpoint }),
    instrumentations: [
      getNodeAutoInstrumentations({
        "@opentelemetry/instrumentation-fs": { enabled: false },
      }),
    ],
  });

  void sdk.start();

  const shutdown = async (signal: string) => {
    try {
      await sdk.shutdown();
      console.log(`OpenTelemetry tracing stopped for api-gateway after ${signal}.`);
    } catch (error) {
      console.error("Failed to shutdown OpenTelemetry tracing for api-gateway.", error);
    } finally {
      process.exit(0);
    }
  };

  process.once("SIGTERM", () => void shutdown("SIGTERM"));
  process.once("SIGINT", () => void shutdown("SIGINT"));
}
