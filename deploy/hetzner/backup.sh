#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
DEPLOY_DIR="$ROOT_DIR/deploy/hetzner"
ENV_FILE="$DEPLOY_DIR/.env"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.yml"
BACKUP_DIR="$DEPLOY_DIR/backups"
TIMESTAMP="$(date -u +%Y%m%d-%H%M%S)"
BACKUP_NAME="vracademy-$TIMESTAMP.bak"
CONTAINER_PATH="/var/opt/mssql/backups/$BACKUP_NAME"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "Missing $ENV_FILE"
  exit 1
fi

mkdir -p "$BACKUP_DIR"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T sqlserver \
  /bin/bash -lc "SQLCMD=/opt/mssql-tools18/bin/sqlcmd; [ -x \"\$SQLCMD\" ] || SQLCMD=/opt/mssql-tools/bin/sqlcmd; \"\$SQLCMD\" -C -S localhost -U sa -P \"\$MSSQL_SA_PASSWORD\" -Q \"BACKUP DATABASE [VRAcademyTraining] TO DISK = N'$CONTAINER_PATH' WITH COPY_ONLY, CHECKSUM, INIT\" -b"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" cp \
  "sqlserver:$CONTAINER_PATH" "$BACKUP_DIR/$BACKUP_NAME"

find "$BACKUP_DIR" -type f -name 'vracademy-*.bak' -mtime +14 -delete
echo "Backup created: $BACKUP_DIR/$BACKUP_NAME"
