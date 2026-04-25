# Windows / PowerShell counterpart to run-all.sh.
# Runs every scenario sequentially and writes JSON summaries to ../results/.

$ErrorActionPreference = 'Stop'

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$ResultsDir = Join-Path $ScriptDir '..\results'
New-Item -ItemType Directory -Force -Path $ResultsDir | Out-Null

if (-not $env:BASE_URL) { $env:BASE_URL = 'http://localhost:9186' }

# Sanity check: hit a gateway endpoint a few times in quick succession to detect
# the default rate limiter (300 req/min). If we see a 429 here, the load test
# is going to fail with rate-limit errors instead of measuring the backend.
Write-Host "Pre-flight: checking gateway rate limit at $env:BASE_URL ..."
$rateLimitHit = $false
for ($i = 0; $i -lt 10; $i++) {
  try {
    $r = Invoke-WebRequest -Uri "$env:BASE_URL/cars/partner-cars?page=1&pageSize=1" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
  } catch {
    if ($_.Exception.Response.StatusCode.value__ -eq 429) { $rateLimitHit = $true; break }
  }
}

if ($rateLimitHit) {
  Write-Host ""
  Write-Host "  Gateway is rate-limiting requests (HTTP 429)." -ForegroundColor Yellow
  Write-Host "  Apply the perf override and restart the gateway, then rerun this script:" -ForegroundColor Yellow
  Write-Host ""
  Write-Host "    docker compose -f docker-compose.yml -f tests/performance/docker-compose.perf.yml up -d api-gateway" -ForegroundColor Cyan
  Write-Host ""
  exit 1
}

$Scenarios = @(
  '01-login',
  '02-catalog',
  '03-car-details',
  '04-price-preview',
  '05-booking-creation',
  '06-ticket-queue'
)

$failed = @()

foreach ($name in $Scenarios) {
  Write-Host ''
  Write-Host '=========================================='
  Write-Host "  Running scenario: $name"
  Write-Host '=========================================='
  & k6 run `
    --summary-export (Join-Path $ResultsDir "$name.json") `
    (Join-Path $ScriptDir "scenarios\$name.js")

  switch ($LASTEXITCODE) {
    0   { }                                # success
    99  { $failed += "$name (thresholds)" }  # threshold breach — record but keep going
    default { $failed += "$name (exit $LASTEXITCODE)" }
  }

  # Brief pause between scenarios so any per-IP counters/connections cool down.
  Start-Sleep -Seconds 5
}

Write-Host ''
Write-Host 'All scenarios finished. Aggregating results...'
& node (Join-Path $ScriptDir 'parse-results.mjs')

if ($failed.Count -gt 0) {
  Write-Host ''
  Write-Host "Some scenarios reported issues: $($failed -join ', ')" -ForegroundColor Yellow
  Write-Host 'See results/summary.md for details.'
}
