import http from 'k6/http';
import { check, fail } from 'k6';
import { BASE_URL } from './config.js';

export function login(email, password) {
  const res = http.post(
    `${BASE_URL}/identity/auth/login`,
    JSON.stringify({ email, password }),
    { headers: { 'Content-Type': 'application/json' }, tags: { name: 'setup_login' } },
  );

  const ok = check(res, {
    'login status is 200': (r) => r.status === 200,
    'login returns accessToken': (r) => !!(r.json() && r.json().accessToken),
  });

  if (!ok) {
    fail(`login failed for ${email}: status=${res.status} body=${res.body}`);
  }

  return res.json().accessToken;
}

export function authHeaders(token) {
  return {
    'Content-Type': 'application/json',
    Authorization: `Bearer ${token}`,
  };
}
