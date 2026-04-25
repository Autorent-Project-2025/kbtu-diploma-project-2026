import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, DEMO_USERS, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';
import { login, authHeaders } from '../lib/auth.js';

export const options = {
  stages: buildStages(LOAD.ticketQueue),
  thresholds: k6Thresholds({ ...LOAD.ticketQueue, ...THRESHOLDS.ticketQueue }),
};

export function setup() {
  // GET /tickets/pending requires the `tickets:view` policy — manager role.
  const token = login(DEMO_USERS.manager.email, DEMO_USERS.manager.password);
  return { token };
}

export default function (data) {
  const res = http.get(
    `${BASE_URL}/tickets/pending`,
    { headers: authHeaders(data.token), tags: { name: 'ticket_queue' } },
  );

  check(res, {
    'status is 200':      (r) => r.status === 200,
    'returns array':      (r) => Array.isArray(r.json()),
  });
}
