import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, DEMO_USERS, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';
import { login, authHeaders } from '../lib/auth.js';
import { fetchCarIds } from '../lib/catalog.js';

export const options = {
  stages: buildStages(LOAD.bookingCreate),
  thresholds: k6Thresholds({ ...LOAD.bookingCreate, ...THRESHOLDS.bookingCreate }),
};

export function setup() {
  const token = login(DEMO_USERS.user.email, DEMO_USERS.user.password);

  // Each run picks a random offset from a 50-year window. Combined with the
  // tight per-iteration footprint below, this makes cross-run collisions on
  // the EXCLUDE constraint statistically negligible (a single test run
  // occupies <1 day of slot space; salt range is ~18 250 days).
  const baseSaltMs = Math.floor(Math.random() * 50 * 365 * 24 * 60 * 60 * 1000);

  return { token, carIds: fetchCarIds(50), baseSaltMs };
}

// Per-iteration slot is 1 minute spacing × 30 seconds duration. Within a run
// (~10k iterations), the test occupies ~7 days of future time — small enough
// that even ten residual runs in DB barely raise collision probability.
function buildWindow(baseSaltMs) {
  const baseFuture = Date.now() + 30 * 24 * 60 * 60 * 1000 + baseSaltMs;
  const offsetMs = (__VU * 1000 + __ITER) * 60 * 1000; // 1 min between slot starts
  const start = new Date(baseFuture + offsetMs);
  const end = new Date(start.getTime() + 30 * 1000); // 30 seconds long
  return { start: start.toISOString(), end: end.toISOString() };
}

export default function (data) {
  const id = data.carIds[(__VU + __ITER) % data.carIds.length];
  const { start, end } = buildWindow(data.baseSaltMs);

  const body = JSON.stringify({
    partnerCarId: id,
    startTime: start,
    endTime: end,
  });

  const res = http.post(
    `${BASE_URL}/bookings`,
    body,
    { headers: authHeaders(data.token), tags: { name: 'booking_create' } },
  );

  check(res, {
    'status is 201':   (r) => r.status === 201,
    'returns booking': (r) => r.json('id') !== null && r.json('id') !== undefined,
  });
}
