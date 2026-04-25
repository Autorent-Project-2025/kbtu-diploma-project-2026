import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';
import { fetchCarIds } from '../lib/catalog.js';

export const options = {
  stages: buildStages(LOAD.pricePreview),
  thresholds: k6Thresholds({ ...LOAD.pricePreview, ...THRESHOLDS.pricePreview }),
};

export function setup() {
  return { carIds: fetchCarIds(50) };
}

export default function (data) {
  const id = data.carIds[(__VU + __ITER) % data.carIds.length];

  // Fixed future window — price preview doesn't persist anything,
  // so identical params across VUs are fine and exercise pricing hot path.
  const start = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000);
  const end = new Date(start.getTime() + 24 * 60 * 60 * 1000);

  const url = `${BASE_URL}/bookings/price-preview?partnerCarId=${id}` +
              `&startTime=${encodeURIComponent(start.toISOString())}` +
              `&endTime=${encodeURIComponent(end.toISOString())}`;

  const res = http.get(url, { tags: { name: 'price_preview' }, timeout: '30s' });

  check(res, {
    'status is 200':   (r) => r.status === 200,
    'has finalPrice':  (r) => typeof r.json('finalPrice') === 'number' && r.json('finalPrice') > 0,
  });
}
