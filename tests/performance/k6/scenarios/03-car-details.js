import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';
import { fetchCarIds } from '../lib/catalog.js';

export const options = {
  stages: buildStages(LOAD.carDetails),
  thresholds: k6Thresholds({ ...LOAD.carDetails, ...THRESHOLDS.carDetails }),
};

export function setup() {
  return { carIds: fetchCarIds(50) };
}

export default function (data) {
  const id = data.carIds[(__VU + __ITER) % data.carIds.length];

  const res = http.get(
    `${BASE_URL}/cars/partner-cars/${id}`,
    { tags: { name: 'car_details' } },
  );

  check(res, {
    'status is 200':  (r) => r.status === 200,
    'returns car id': (r) => r.json('id') === id,
  });
}
