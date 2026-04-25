# Performance Testing

Backend load tests for the key user-facing scenarios listed in thesis section
**4.1 Performance testing**. Implemented with [k6](https://k6.io/), one script
per scenario plus a runner that aggregates results into a Markdown table that
can be pasted directly into the thesis.

## Scenarios

| # | Scenario | HTTP | Endpoint (via gateway) | Auth | Default VUs |
|---|---|---|---|---|---|
| 1 | Login | POST | `/identity/auth/login` | none | 50 |
| 2 | Car catalog loading | GET | `/cars/partner-cars?page=…&pageSize=…` | none | 50 |
| 3 | Car details loading | GET | `/cars/partner-cars/{id}` | none | 50 |
| 4 | Price preview | GET | `/bookings/price-preview?partnerCarId=…&startTime=…&endTime=…` | none | 50 |
| 5 | Booking creation | POST | `/bookings` | user JWT | 20 |
| 6 | Ticket queue loading | GET | `/tickets/pending` | manager JWT | 20 |

Each scenario follows the same load profile: ramp-up → 1 minute steady at the
target VU count → ramp-down. All knobs (VUs, durations, p95 budgets, error
budgets) are centralized in [k6/lib/config.js](k6/lib/config.js).

## Metrics collected

Per scenario (extracted from k6's built-in metrics):

- `http_req_duration.avg` — average response time
- `http_req_duration.p(95)` — 95th-percentile response time
- `http_req_failed.rate` — error rate (HTTP 4xx/5xx + transport errors)
- `http_reqs.rate` — requests per second
- `http_reqs.count` — total requests issued

Pass/fail criteria (per scenario, in `lib/config.js → THRESHOLDS`):

- `p95 < scenario budget`
- `error rate < scenario error budget`

A scenario is reported as **Passed** when both conditions hold.

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/) on `PATH`
- Node.js 18+ (only used by `parse-results.mjs` to aggregate the JSON outputs)
- The full stack running locally: `docker compose up -d` from the repo root

### Disable gateway rate limiting for the test run

The api-gateway is configured for production with `RATE_LIMIT_MAX_REQUESTS=300`
per 60s window. That cap is reached within seconds at 50 VUs and the test
ends up measuring the rate limiter, not the backend. Apply the perf override
shipped in this folder before running the suite:

```bash
docker compose -f docker-compose.yml -f tests/performance/docker-compose.perf.yml up -d api-gateway
```

To revert to production-equivalent rate limits afterwards:

```bash
docker compose up -d api-gateway
```

The `run-all.*` scripts perform a pre-flight check and abort with this exact
hint if they detect HTTP 429 from the gateway.

### Base URL and credentials

The default gateway URL is `http://localhost:9186`. Override via `BASE_URL`:

```bash
BASE_URL=https://staging.example.com k6 run k6/scenarios/01-login.js
```

Demo credentials default to the seeded ones (`user@autorent.local` /
`DemoUser123!`, `manager@autorent.local` / `DemoManager123!`). Override via
`DEMO_USER_EMAIL` / `DEMO_USER_PASSWORD` / `DEMO_MANAGER_EMAIL` /
`DEMO_MANAGER_PASSWORD`.

## How to run

### One scenario at a time

```bash
k6 run k6/scenarios/01-login.js
k6 run k6/scenarios/02-catalog.js
# … etc
```

k6 prints a live summary at the end of each run. Thresholds are configured per
scenario, so a failed run exits with code 99.

### All scenarios + aggregated table (for the thesis)

```bash
# Linux / macOS / Git Bash on Windows:
bash k6/run-all.sh

# PowerShell:
powershell -ExecutionPolicy Bypass -File k6/run-all.ps1
```

This runs every scenario in sequence, writes a per-scenario JSON summary to
`results/0X-*.json`, then runs `parse-results.mjs` which produces:

- `results/summary.md` — Markdown table in the exact thesis format
- `results/summary.json` — machine-readable aggregate

Paste the Markdown table from `summary.md` straight into thesis section 4.1.

## File layout

```
tests/performance/
├── k6/
│   ├── lib/
│   │   ├── config.js          # BASE_URL, demo users, load profiles, thresholds
│   │   ├── auth.js            # login() helper, used by setup() in auth scenarios
│   │   └── catalog.js         # fetchCarIds() helper, supplies real IDs to scenarios
│   ├── scenarios/
│   │   ├── 01-login.js
│   │   ├── 02-catalog.js
│   │   ├── 03-car-details.js
│   │   ├── 04-price-preview.js
│   │   ├── 05-booking-creation.js
│   │   └── 06-ticket-queue.js
│   ├── run-all.sh             # Bash runner
│   ├── run-all.ps1            # PowerShell runner
│   └── parse-results.mjs      # Aggregates JSON summaries → summary.md/.json
└── results/                   # Output of run-all.* (gitignored except .gitkeep)
```

## Notes on test design

- **Booking creation** uses far-future, per-VU disjoint time windows
  (`base + VU*offset`) so the service's availability check doesn't reject
  parallel iterations as conflicts. This keeps the test focused on creation
  latency rather than retry behavior.
- **Car details** and **price preview** pull a real list of `partnerCarId`s
  in `setup()` instead of hard-coding seed IDs, so the scripts survive
  reseeding and can be reused against staging.
- **Ticket queue** logs in once as the seeded manager (`tickets:view`
  policy) and reuses the JWT for the duration of the test.
- **Login** intentionally re-authenticates on every iteration — that's the
  workload we want to measure.
