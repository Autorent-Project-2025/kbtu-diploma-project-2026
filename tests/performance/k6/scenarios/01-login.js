import http from 'k6/http';
import { check } from 'k6';
import { BASE_URL, DEMO_USERS, LOAD, THRESHOLDS, buildStages, k6Thresholds } from '../lib/config.js';

export const options = {
  stages: buildStages(LOAD.login),
  thresholds: k6Thresholds({ ...LOAD.login, ...THRESHOLDS.login }),
};

const payload = JSON.stringify({
  email: DEMO_USERS.user.email,
  password: DEMO_USERS.user.password,
});

const params = {
  headers: { 'Content-Type': 'application/json' },
  tags: { name: 'login' },
};

export default function () {
  const res = http.post(`${BASE_URL}/identity/auth/login`, payload, params);

  check(res, {
    'status is 200':    (r) => r.status === 200,
    'has access token': (r) => !!(r.json() && r.json().accessToken),
  });
}
