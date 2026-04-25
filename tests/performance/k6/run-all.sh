#!/usr/bin/env bash
# Sequentially runs every k6 scenario and writes a JSON summary per scenario
# into ../results/. Re-run after making backend changes; the parser
# (parse-results.mjs) consumes those JSON files to produce the thesis table.
set -uo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$SCRIPT_DIR/../results"
mkdir -p "$RESULTS_DIR"

BASE_URL="${BASE_URL:-http://localhost:9186}"
export BASE_URL

# Pre-flight: detect the default 300 req/min gateway rate limiter before
# launching any scenario. If we get a 429, point the user at the perf override.
echo "Pre-flight: checking gateway rate limit at $BASE_URL ..."
rate_limit_hit=0
for i in $(seq 1 10); do
  status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 \
    "$BASE_URL/cars/partner-cars?page=1&pageSize=1" || echo "000")
  if [[ "$status" == "429" ]]; then
    rate_limit_hit=1
    break
  fi
done

if [[ "$rate_limit_hit" == "1" ]]; then
  cat <<EOF

  Gateway is rate-limiting requests (HTTP 429).
  Apply the perf override and restart the gateway, then rerun this script:

    docker compose -f docker-compose.yml -f tests/performance/docker-compose.perf.yml up -d api-gateway

EOF
  exit 1
fi

SCENARIOS=(
  "01-login"
  "02-catalog"
  "03-car-details"
  "04-price-preview"
  "05-booking-creation"
  "06-ticket-queue"
)

failed=()

for name in "${SCENARIOS[@]}"; do
  echo ""
  echo "=========================================="
  echo "  Running scenario: $name"
  echo "=========================================="
  k6 run \
    --summary-export "$RESULTS_DIR/$name.json" \
    "$SCRIPT_DIR/scenarios/$name.js"

  case $? in
    0)  ;;
    99) failed+=("$name (thresholds)") ;;
    *)  failed+=("$name (exit $?)") ;;
  esac

  # Brief pause between scenarios so per-IP counters/connections cool down.
  sleep 5
done

echo ""
echo "All scenarios finished. Aggregating results..."
node "$SCRIPT_DIR/parse-results.mjs"

if [[ ${#failed[@]} -gt 0 ]]; then
  echo ""
  echo "Some scenarios reported issues: ${failed[*]}"
  echo "See results/summary.md for details."
fi
