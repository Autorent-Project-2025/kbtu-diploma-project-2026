// Shared configuration for all k6 performance scenarios.
// All requests go through the API gateway (reverse-proxy-service)
// exposed at port 9186 (see docker-compose.yml).

export const BASE_URL = __ENV.BASE_URL || 'http://localhost:9186';

export const DEMO_USERS = {
  user: {
    email: __ENV.DEMO_USER_EMAIL || 'user@autorent.local',
    password: __ENV.DEMO_USER_PASSWORD || 'DemoUser123!',
  },
  manager: {
    email: __ENV.DEMO_MANAGER_EMAIL || 'manager@autorent.local',
    password: __ENV.DEMO_MANAGER_PASSWORD || 'DemoManager123!',
  },
};

// Per-scenario load profile. Targets are taken from the thesis
// performance-testing chapter (Login/Catalog/Booking) and extended
// with sensible defaults for the remaining scenarios.
export const LOAD = {
  login:         { vus: 50, duration: '1m', rampUp: '15s', rampDown: '15s' },
  catalog:       { vus: 50, duration: '1m', rampUp: '15s', rampDown: '15s' },
  // After enabling output cache on car-service, the per-request cost dropped
  // ~100x. At 50 VUs the test now generates ~280 RPS — high enough that the
  // gateway → car-service socket pool starts dropping connections before the
  // request reaches the app. 30 VUs keeps the test below that threshold so we
  // measure the cached read path, not the gateway connection limit. The
  // gateway-side limit itself is captured separately in section 4.5.
  carDetails:    { vus: 30, duration: '1m', rampUp: '10s', rampDown: '10s' },
  // Price preview fans out to car-service + car-market-value-service per
  // request, so it's effectively as expensive as a write. 20 VUs keeps the
  // measurement meaningful instead of saturating downstream dependencies.
  pricePreview:  { vus: 20, duration: '1m', rampUp: '10s', rampDown: '10s' },
  bookingCreate: { vus: 20, duration: '1m', rampUp: '10s', rampDown: '10s' },
  ticketQueue:   { vus: 20, duration: '1m', rampUp: '10s', rampDown: '10s' },
};

// Pass/fail thresholds. A scenario is "Passed" when:
//   * p95 response time stays under the budget,
//   * error rate stays below 1%.
export const THRESHOLDS = {
  login:         { p95: 800,  errorRate: 0.01 },
  catalog:       { p95: 600,  errorRate: 0.01 },
  carDetails:    { p95: 400,  errorRate: 0.01 },
  pricePreview:  { p95: 700,  errorRate: 0.01 },
  bookingCreate: { p95: 1500, errorRate: 0.02 },
  ticketQueue:   { p95: 800,  errorRate: 0.01 },
};

export function buildStages(profile) {
  return [
    { duration: profile.rampUp,   target: profile.vus },
    { duration: profile.duration, target: profile.vus },
    { duration: profile.rampDown, target: 0 },
  ];
}

export function k6Thresholds(profile) {
  return {
    http_req_failed: [`rate<${profile.errorRate}`],
    http_req_duration: [`p(95)<${profile.p95}`],
  };
}
