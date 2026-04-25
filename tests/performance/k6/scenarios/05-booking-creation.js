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
  return { token, carIds: fetchCarIds(50) };
}

// Booking availability is enforced by the service, so each VU/iteration
// must request a non-overlapping time window. We pick a far-future date
// and offset hours by VU * iter to keep slots disjoint.
function buildWindow() {
  const baseFuture = Date.now() + 30 * 24 * 60 * 60 * 1000; // 30 days ahead
  const offsetMs = (__VU * 1000 + __ITER) * 2 * 60 * 60 * 1000; // 2h per slot
  const start = new Date(baseFuture + offsetMs);
  const end = new Date(start.getTime() + 60 * 60 * 1000); // 1 hour
  return { start: start.toISOString(), end: end.toISOString() };
}

export default function (data) {
  const id = data.carIds[(__VU + __ITER) % data.carIds.length];
  const { start, end } = buildWindow();

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
