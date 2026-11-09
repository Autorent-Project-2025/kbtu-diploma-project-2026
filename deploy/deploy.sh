#!/usr/bin/env bash
# One-command deploy of the AutoRent backend on a single Ubuntu VM.
#
#   ./deploy/deploy.sh            # first run or redeploy of the current code
#   ./deploy/deploy.sh --pull     # git pull first, then redeploy
#
# Idempotent: secrets are generated once into deploy/secrets/ and re-applied
# on every run, so it is safe to run again after each git pull.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
SECRETS_DIR="$ROOT/deploy/secrets"
COMPOSE=(docker compose -f docker-compose.yml -f docker-compose.prod.yml)

log() { printf '\n\033[1;32m==> %s\033[0m\n' "$*"; }

# ---------------------------------------------------------------- 1. Docker
if ! command -v docker >/dev/null 2>&1; then
  log "Installing Docker"
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker "$USER" || true
fi
if ! docker info >/dev/null 2>&1; then
  COMPOSE=(sudo "${COMPOSE[@]}")
fi

# ------------------------------------------------- 2. Firewall (Oracle Ubuntu)
# Oracle's Ubuntu images ship an iptables REJECT rule; open 80/443 before it.
if command -v iptables >/dev/null 2>&1; then
  for port in 80 443; do
    if ! sudo iptables -C INPUT -p tcp --dport "$port" -j ACCEPT 2>/dev/null; then
      log "Opening port $port in iptables"
      sudo iptables -I INPUT 1 -p tcp --dport "$port" -j ACCEPT
    fi
  done
  sudo iptables -C INPUT -p udp --dport 443 -j ACCEPT 2>/dev/null \
    || sudo iptables -I INPUT 1 -p udp --dport 443 -j ACCEPT
  if command -v netfilter-persistent >/dev/null 2>&1; then
    sudo netfilter-persistent save >/dev/null
  fi
fi

# ------------------------------------------------------------ 3. Root .env
if [[ ! -f .env ]]; then
  log "Creating .env"
  PUBLIC_IP="$(curl -fsS https://api.ipify.org)"
  API_DOMAIN="api.${PUBLIC_IP//./-}.sslip.io"
  cp .env.example .env
  cat >> .env <<EOF

# --- production (deploy/deploy.sh) ---
API_DOMAIN=${API_DOMAIN}
EXTERNAL_FRONTEND_URL=https://autorent-external.vercel.app
INTERNAL_FRONTEND_URL=https://autorent-internal.vercel.app
SUPERADMIN_FRONTEND_URL=https://autorent-superadmin.vercel.app
EOF
  sed -i "s|^FILE_SERVICE_PUBLIC_BASE_URL=.*|FILE_SERVICE_PUBLIC_BASE_URL=https://${API_DOMAIN}/files|" .env
  sed -i "s|^IMAGE_SERVICE_PUBLIC_BASE_URL=.*|IMAGE_SERVICE_PUBLIC_BASE_URL=https://${API_DOMAIN}/internal|" .env
  echo "API domain: https://${API_DOMAIN}"
  echo "Check the *_FRONTEND_URL values in .env match your Vercel URLs."
fi

# ------------------------------------------------- 4. Pristine tree (+ pull)
# Secrets are written into tracked files below, so restore them first.
git checkout -- docker-compose.yml backend
if [[ "${1:-}" == "--pull" ]]; then
  log "Pulling latest code"
  git pull --ff-only
fi

# ------------------------------------------------------------ 5. Secrets
mkdir -p "$SECRETS_DIR"
chmod 700 "$SECRETS_DIR"
if [[ ! -f "$SECRETS_DIR/jwt_private.pem" ]]; then
  log "Generating new JWT key pair (the one in the repo is public)"
  openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out "$SECRETS_DIR/jwt_private.pem"
  openssl pkey -in "$SECRETS_DIR/jwt_private.pem" -pubout -out "$SECRETS_DIR/jwt_public.pem"
fi

log "Applying secrets to compose and service env files"
python3 - "$ROOT" "$SECRETS_DIR" <<'PY'
import json, pathlib, re, secrets, sys

root, secrets_dir = pathlib.Path(sys.argv[1]), pathlib.Path(sys.argv[2])
keys_file = secrets_dir / "internal_keys.json"
keys = json.loads(keys_file.read_text()) if keys_file.exists() else {}

def esc(pem: str) -> str:
    return pem.strip().replace("\n", "\\n") + "\\n"

private = esc((secrets_dir / "jwt_private.pem").read_text())
public = esc((secrets_dir / "jwt_public.pem").read_text())

token = re.compile(r"local-[a-z0-9-]+-key")
def new_key(m):
    return keys.setdefault(m.group(0), secrets.token_urlsafe(32))

files = [root / "docker-compose.yml", *root.glob("backend/**/.env.example")]
for f in files:
    if "node_modules" in f.parts:
        continue
    text = f.read_text()
    out = token.sub(new_key, text)
    out = re.sub(r"(?m)^(Jwt__PrivateKey)=.*$", lambda m: f"{m.group(1)}={private}", out)
    out = re.sub(r"(?m)^(Jwt__PublicKey|JWT_PUBLIC_KEY)=.*$", lambda m: f"{m.group(1)}={public}", out)
    if out != text:
        f.write_text(out)

keys_file.write_text(json.dumps(keys, indent=2))
keys_file.chmod(0o600)
print(f"rotated {len(keys)} internal API keys and the JWT key pair")
PY

# ------------------------------------------------------------ 6. Start
log "Building and starting containers (first build takes 20-40 min on ARM)"
"${COMPOSE[@]}" up -d --build --remove-orphans

API_DOMAIN="$(grep -E '^API_DOMAIN=' .env | cut -d= -f2)"
log "Done"
"${COMPOSE[@]}" ps --format 'table {{.Service}}\t{{.Status}}'
echo
echo "API:    https://${API_DOMAIN}/healthz"
echo "Logs:   ${COMPOSE[*]} logs -f api-gateway"
