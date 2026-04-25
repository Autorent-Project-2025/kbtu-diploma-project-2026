// Generates two SVG bar charts comparing baseline vs final performance
// across all six scenarios, ready to drop into thesis section 4.1:
//
//   results/figure-4-1-p95.svg    — P95 response time, log scale
//   results/figure-4-2-errors.svg — Error rate, linear scale
//
// No external dependencies — pure Node + fs. Run after a full k6 suite:
//
//   node tests/performance/k6/generate-charts.mjs

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const resultsDir = path.resolve(__dirname, '..', 'results');

const SCENARIO_FILES = [
  { scenario: 'Login',            file: '01-login.json' },
  { scenario: 'Catalog',          file: '02-catalog.json' },
  { scenario: 'Car details',      file: '03-car-details.json' },
  { scenario: 'Price preview',    file: '04-price-preview.json' },
  { scenario: 'Booking creation', file: '05-booking-creation.json' },
  { scenario: 'Ticket queue',     file: '06-ticket-queue.json' },
];

function readBaseline() {
  const raw = JSON.parse(fs.readFileSync(path.join(resultsDir, 'baseline.json'), 'utf8'));
  // Keyed by scenario for easy lookup; align label "Catalog loading" → "Catalog".
  const byKey = new Map();
  for (const row of raw) {
    const key = row.scenario.replace(/\s+loading$/i, '');
    byKey.set(key, { p95: row.p95Ms, errorRate: row.errorRate });
  }
  return byKey;
}

function readFinal() {
  return SCENARIO_FILES.map(({ scenario, file }) => {
    const data = JSON.parse(fs.readFileSync(path.join(resultsDir, file), 'utf8'));
    const dur = data?.metrics?.http_req_duration ?? {};
    const fail = data?.metrics?.http_req_failed ?? {};
    return {
      scenario,
      p95: dur['p(95)'] ?? 0,
      errorRate: fail.value ?? 0,
    };
  });
}

// SVG layout constants — all coordinates in pixels.
const W = 900;
const H = 540;
const MARGIN = { top: 64, right: 32, bottom: 132, left: 80 };
const PLOT_W = W - MARGIN.left - MARGIN.right;
const PLOT_H = H - MARGIN.top - MARGIN.bottom;

const COLOR_BASELINE = '#d9534f'; // red
const COLOR_FINAL = '#5cb85c'; // green

function escapeXml(s) {
  return String(s).replace(/[<>&"']/g, c =>
    ({ '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;', "'": '&apos;' }[c]));
}

// Log-scale Y mapping for P95 chart.
function makeLogY(min, max) {
  const lmin = Math.log10(min);
  const lmax = Math.log10(max);
  return value => {
    const v = Math.max(value, min);
    const t = (Math.log10(v) - lmin) / (lmax - lmin);
    return MARGIN.top + PLOT_H * (1 - t);
  };
}

// Linear Y mapping for error-rate chart.
function makeLinearY(min, max) {
  return value => {
    const t = (value - min) / (max - min);
    return MARGIN.top + PLOT_H * (1 - t);
  };
}

function buildChart({ title, yLabel, scenarios, series, yMap, yTicks, valueFormat }) {
  const groupWidth = PLOT_W / scenarios.length;
  const barWidth = Math.min(groupWidth * 0.35, 60);
  const barGap = 4;

  const parts = [];
  parts.push(`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${W} ${H}" font-family="Arial, sans-serif" font-size="13">`);
  parts.push(`<rect width="${W}" height="${H}" fill="white"/>`);

  // Title
  parts.push(`<text x="${W / 2}" y="28" text-anchor="middle" font-size="16" font-weight="bold">${escapeXml(title)}</text>`);

  // Y-axis label (rotated)
  parts.push(`<text x="20" y="${MARGIN.top + PLOT_H / 2}" text-anchor="middle" transform="rotate(-90 20 ${MARGIN.top + PLOT_H / 2})">${escapeXml(yLabel)}</text>`);

  // Plot border
  parts.push(`<rect x="${MARGIN.left}" y="${MARGIN.top}" width="${PLOT_W}" height="${PLOT_H}" fill="none" stroke="#ccc"/>`);

  // Y-axis ticks + grid
  for (const tick of yTicks) {
    const y = yMap(tick);
    parts.push(`<line x1="${MARGIN.left}" y1="${y}" x2="${MARGIN.left + PLOT_W}" y2="${y}" stroke="#eee"/>`);
    parts.push(`<text x="${MARGIN.left - 8}" y="${y + 4}" text-anchor="end" fill="#444">${escapeXml(valueFormat(tick))}</text>`);
  }

  // Bars
  scenarios.forEach((label, i) => {
    const groupCenter = MARGIN.left + (i + 0.5) * groupWidth;
    const x1 = groupCenter - barWidth - barGap / 2;
    const x2 = groupCenter + barGap / 2;

    const v1 = series[0].values[i];
    const v2 = series[1].values[i];
    const y1 = yMap(v1);
    const y2 = yMap(v2);
    const yBase = yMap(yTicks[0]);

    // Baseline bar
    if (v1 > 0) {
      parts.push(`<rect x="${x1}" y="${y1}" width="${barWidth}" height="${yBase - y1}" fill="${series[0].color}"/>`);
      parts.push(`<text x="${x1 + barWidth / 2}" y="${y1 - 4}" text-anchor="middle" font-size="11" fill="#222">${escapeXml(valueFormat(v1))}</text>`);
    }
    // Final bar
    if (v2 > 0) {
      parts.push(`<rect x="${x2}" y="${y2}" width="${barWidth}" height="${yBase - y2}" fill="${series[1].color}"/>`);
      parts.push(`<text x="${x2 + barWidth / 2}" y="${y2 - 4}" text-anchor="middle" font-size="11" fill="#222">${escapeXml(valueFormat(v2))}</text>`);
    } else {
      // Mark zero values explicitly so they don't disappear visually.
      parts.push(`<text x="${x2 + barWidth / 2}" y="${yBase - 4}" text-anchor="middle" font-size="11" fill="#777">${escapeXml(valueFormat(0))}</text>`);
    }

    // X-axis label, rotated for readability
    const labelY = MARGIN.top + PLOT_H + 24;
    parts.push(`<text x="${groupCenter}" y="${labelY}" text-anchor="end" transform="rotate(-30 ${groupCenter} ${labelY})">${escapeXml(label)}</text>`);
  });

  // Legend — horizontal, centered below the rotated x-axis labels so it
  // never collides with bars regardless of their heights.
  const legendY = MARGIN.top + PLOT_H + 70;
  const swatch1Width = 14;
  const swatch2Width = 14;
  const text1Width = series[0].name.length * 7;
  const text2Width = series[1].name.length * 7;
  const gap = 24;
  const totalWidth = swatch1Width + 6 + text1Width + gap + swatch2Width + 6 + text2Width;
  const legendStartX = (W - totalWidth) / 2;

  let cursor = legendStartX;
  parts.push(`<rect x="${cursor}" y="${legendY - 11}" width="${swatch1Width}" height="14" fill="${series[0].color}"/>`);
  cursor += swatch1Width + 6;
  parts.push(`<text x="${cursor}" y="${legendY}">${escapeXml(series[0].name)}</text>`);
  cursor += text1Width + gap;
  parts.push(`<rect x="${cursor}" y="${legendY - 11}" width="${swatch2Width}" height="14" fill="${series[1].color}"/>`);
  cursor += swatch2Width + 6;
  parts.push(`<text x="${cursor}" y="${legendY}">${escapeXml(series[1].name)}</text>`);

  parts.push('</svg>');
  return parts.join('\n');
}

function formatMs(v) {
  if (v >= 1000) return `${(v / 1000).toFixed(1)}s`;
  return `${Math.round(v)}ms`;
}

function formatPct(v) {
  return `${(v * 100).toFixed(1)}%`;
}

function main() {
  const baseline = readBaseline();
  const final = readFinal();

  const scenarios = final.map(f => f.scenario);
  const baselineP95 = scenarios.map(s => baseline.get(s)?.p95 ?? 0);
  const baselineErr = scenarios.map(s => baseline.get(s)?.errorRate ?? 0);
  const finalP95 = final.map(f => f.p95);
  const finalErr = final.map(f => f.errorRate);

  // Figure 4.1 — P95 log scale
  const p95Svg = buildChart({
    title: 'Figure 4.1 — P95 response time: baseline vs final (log scale)',
    yLabel: 'P95 response time',
    scenarios,
    series: [
      { name: 'Baseline (no optimization)', color: COLOR_BASELINE, values: baselineP95 },
      { name: 'Final (after optimization)', color: COLOR_FINAL,    values: finalP95 },
    ],
    yMap: makeLogY(100, 100000),
    yTicks: [100, 1000, 10000, 100000],
    valueFormat: formatMs,
  });
  fs.writeFileSync(path.join(resultsDir, 'figure-4-1-p95.svg'), p95Svg);

  // Figure 4.2 — Error rate linear scale
  const errSvg = buildChart({
    title: 'Figure 4.2 — Error rate: baseline vs final',
    yLabel: 'Error rate',
    scenarios,
    series: [
      { name: 'Baseline (no optimization)', color: COLOR_BASELINE, values: baselineErr },
      { name: 'Final (after optimization)', color: COLOR_FINAL,    values: finalErr },
    ],
    yMap: makeLinearY(0, 1),
    yTicks: [0, 0.25, 0.5, 0.75, 1],
    valueFormat: formatPct,
  });
  fs.writeFileSync(path.join(resultsDir, 'figure-4-2-errors.svg'), errSvg);

  console.log('Wrote:');
  console.log(`  ${path.join(resultsDir, 'figure-4-1-p95.svg')}`);
  console.log(`  ${path.join(resultsDir, 'figure-4-2-errors.svg')}`);
  console.log('');
  console.log('Data used:');
  console.log('  Scenario           Baseline p95   Final p95   Baseline err  Final err');
  for (let i = 0; i < scenarios.length; i++) {
    const s = scenarios[i].padEnd(18);
    console.log(`  ${s} ${formatMs(baselineP95[i]).padStart(10)}    ${formatMs(finalP95[i]).padStart(8)}     ${formatPct(baselineErr[i]).padStart(7)}    ${formatPct(finalErr[i]).padStart(7)}`);
  }
}

main();
