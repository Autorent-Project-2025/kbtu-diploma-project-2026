// Aggregates the per-scenario JSON summaries written by `k6 run --summary-export`
// into a single Markdown report formatted exactly like the thesis table:
//
//   Test scenario | Users | Avg response time | P95 response time | Error rate | Result
//
// Output: ../results/summary.md and ../results/summary.json

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const resultsDir = path.resolve(__dirname, '..', 'results');

const SCENARIOS = [
  { file: '01-login.json',            label: 'Login',             users: 50, p95Budget: 800,  errorBudget: 0.01 },
  { file: '02-catalog.json',          label: 'Catalog loading',   users: 50, p95Budget: 600,  errorBudget: 0.01 },
  { file: '03-car-details.json',      label: 'Car details',       users: 50, p95Budget: 400,  errorBudget: 0.01 },
  { file: '04-price-preview.json',    label: 'Price preview',     users: 50, p95Budget: 700,  errorBudget: 0.01 },
  { file: '05-booking-creation.json', label: 'Booking creation',  users: 20, p95Budget: 1500, errorBudget: 0.02 },
  { file: '06-ticket-queue.json',     label: 'Ticket queue',      users: 20, p95Budget: 800,  errorBudget: 0.01 },
];

const fmtMs = (ms) => `${ms.toFixed(1)} ms`;
const fmtPct = (rate) => `${(rate * 100).toFixed(2)} %`;

function readSummary(file) {
  const fullPath = path.join(resultsDir, file);
  if (!fs.existsSync(fullPath)) return null;
  try {
    return JSON.parse(fs.readFileSync(fullPath, 'utf-8'));
  } catch (err) {
    console.error(`Failed to parse ${file}:`, err.message);
    return null;
  }
}

function extract(summary) {
  // k6 --summary-export writes metrics flat: metrics.{name}.{stat}
  // (no `.values` wrapper). For http_req_failed the rate is stored as `.value`.
  const dur = summary?.metrics?.http_req_duration || {};
  const failed = summary?.metrics?.http_req_failed || {};
  const reqs = summary?.metrics?.http_reqs || {};

  return {
    avg: dur.avg ?? 0,
    p95: dur['p(95)'] ?? 0,
    errorRate: failed.value ?? failed.rate ?? 0,
    rps: reqs.rate ?? 0,
    count: reqs.count ?? 0,
  };
}

const rows = [];
const json = [];

for (const sc of SCENARIOS) {
  const summary = readSummary(sc.file);
  if (!summary) {
    rows.push(`| ${sc.label} | ${sc.users} | n/a | n/a | n/a | Not run |`);
    json.push({ scenario: sc.label, users: sc.users, status: 'Not run' });
    continue;
  }

  const m = extract(summary);
  const passed = m.p95 <= sc.p95Budget && m.errorRate <= sc.errorBudget;

  rows.push(
    `| ${sc.label} | ${sc.users} | ${fmtMs(m.avg)} | ${fmtMs(m.p95)} | ${fmtPct(m.errorRate)} | ${passed ? 'Passed' : 'Failed'} |`,
  );

  json.push({
    scenario: sc.label,
    users: sc.users,
    avgMs: +m.avg.toFixed(1),
    p95Ms: +m.p95.toFixed(1),
    errorRate: +m.errorRate.toFixed(4),
    requestsPerSecond: +m.rps.toFixed(2),
    requestCount: m.count,
    p95Budget: sc.p95Budget,
    errorBudget: sc.errorBudget,
    result: passed ? 'Passed' : 'Failed',
  });
}

const md = [
  '# Performance Testing Results',
  '',
  `Generated: ${new Date().toISOString()}`,
  `Tool: k6`,
  `Gateway: ${process.env.BASE_URL || 'http://localhost:9186'}`,
  '',
  '## Summary',
  '',
  '| Test scenario | Users | Avg response time | P95 response time | Error rate | Result |',
  '|---|---|---|---|---|---|',
  ...rows,
  '',
  '## Notes',
  '',
  '- "Result = Passed" means p95 stayed under the budget and error rate stayed under the error budget for that scenario.',
  '- Budgets are defined in [lib/config.js](../k6/lib/config.js) under THRESHOLDS.',
  '- Raw JSON summaries per scenario live next to this file (`0X-*.json`).',
  '',
].join('\n');

fs.writeFileSync(path.join(resultsDir, 'summary.md'), md);
fs.writeFileSync(path.join(resultsDir, 'summary.json'), JSON.stringify(json, null, 2));

console.log(md);
console.log(`\nWrote: ${path.join(resultsDir, 'summary.md')}`);
console.log(`Wrote: ${path.join(resultsDir, 'summary.json')}`);
