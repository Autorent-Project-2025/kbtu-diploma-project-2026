import http from 'k6/http';
import { check, fail } from 'k6';
import { BASE_URL } from './config.js';

// Pulls a sample of partner-car IDs from the public catalog.
// Used by scenarios that need a valid carId without hard-coding seed data.
export function fetchCarIds(pageSize = 50) {
  const res = http.get(
    `${BASE_URL}/cars/partner-cars?page=1&pageSize=${pageSize}`,
    { tags: { name: 'setup_fetch_catalog' } },
  );

  const ok = check(res, {
    'catalog status is 200': (r) => r.status === 200,
    'catalog has items':     (r) => Array.isArray(r.json('items')) && r.json('items').length > 0,
  });

  if (!ok) {
    fail(`catalog fetch failed: status=${res.status} body=${res.body}`);
  }

  return res.json('items').map((c) => c.id);
}
