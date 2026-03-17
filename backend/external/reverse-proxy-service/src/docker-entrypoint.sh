#!/bin/sh
set -eu

if [ "${TLS_ENABLED:-true}" = "true" ]; then
  cert_path="${TLS_CERT_PATH:-/tmp/autorent-edge.crt}"
  key_path="${TLS_KEY_PATH:-/tmp/autorent-edge.key}"
  cert_cn="${TLS_CERT_CN:-localhost}"
  cert_days="${TLS_CERT_DAYS:-30}"

  if [ ! -f "$cert_path" ] || [ ! -f "$key_path" ]; then
    mkdir -p "$(dirname "$cert_path")" "$(dirname "$key_path")"

    openssl req \
      -x509 \
      -newkey rsa:2048 \
      -nodes \
      -keyout "$key_path" \
      -out "$cert_path" \
      -days "$cert_days" \
      -subj "/CN=$cert_cn" \
      >/dev/null 2>&1
  fi
fi

exec npm run start
