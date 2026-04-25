import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';

export const options = {
  stages: buildStages(LOAD.catalog),
  thresholds: k6Thresholds({ ...LOAD.catalog, ...THRESHOLDS.catalog }),
};

const PAGE_SIZE = 10;

export default function () {
  // Page index varies per VU/iteration to exercise pagination instead of
  // hammering only the first page (which the gateway/cache might serve cheaply).
  const page = ((__VU + __ITER) % 5) + 1;

  const res = http.get(
    `${BASE_URL}/cars/partner-cars?page=${page}&pageSize=${PAGE_SIZE}`,
    { tags: { name: 'catalog' } },
  );

  check(res, {
    'status is 200': (r) => r.status === 200,
    'has items':     (r) => Array.isArray(r.json('items')),
  });
}
