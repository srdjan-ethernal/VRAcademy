#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DEPLOY_DIR="$ROOT_DIR/deploy/hetzner"
ENV_FILE="$DEPLOY_DIR/.env"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.yml"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Missing $ENV_FILE"
  echo "Run: cp deploy/hetzner/.env.example deploy/hetzner/.env"
  exit 1
fi

if ! grep -Eq '^APP_DOMAIN=[A-Za-z0-9.-]+$' "$ENV_FILE"; then
  echo "Set APP_DOMAIN in deploy/hetzner/.env"
  exit 1
fi

if ! grep -Eq '^WWW_DOMAIN=[A-Za-z0-9.-]+$' "$ENV_FILE"; then
  echo "Set WWW_DOMAIN in deploy/hetzner/.env"
  exit 1
fi

if ! grep -Eq '^MSSQL_SA_PASSWORD=.{16,}$' "$ENV_FILE" || grep -q '^MSSQL_SA_PASSWORD=CHANGE_ME' "$ENV_FILE"; then
  echo "Set a strong MSSQL_SA_PASSWORD with at least 16 characters."
  exit 1
fi

if grep -Eq '^MSSQL_SA_PASSWORD=.*;' "$ENV_FILE"; then
  echo "MSSQL_SA_PASSWORD must not contain a semicolon."
  exit 1
fi

cd "$ROOT_DIR"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" pull sqlserver caddy
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build --pull app
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --remove-orphans

echo "Waiting for the application health endpoint..."
for attempt in {1..30}; do
  if curl --fail --silent --show-error http://127.0.0.1:7860/api/health >/dev/null; then
    echo "VR Academy is healthy."
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
    exit 0
  fi

  if [[ "$attempt" -eq 30 ]]; then
    echo "Application health check failed. Recent logs:"
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" logs --tail=100 app sqlserver
    exit 1
  fi

  sleep 5
done
