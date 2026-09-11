#!/bin/bash
# db-backup.sh — Sauvegarde de la base ChronoKraft
#
# Usage:
#   ./db-backup.sh
#   ./db-backup.sh "avant-migration-worker-skill"
#
# Le mot de passe est demandé de façon sécurisée, sauf si la variable
# d'environnement CHRONOKRAFT_DB_PASSWORD est déjà définie.

set -euo pipefail

DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="chronok_db"
DB_USER="chronok_adm"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="$SCRIPT_DIR/../backups"
mkdir -p "$BACKUP_DIR"

LABEL="${1:-}"
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
if [ -n "$LABEL" ]; then
    FILENAME="chronok_db_${LABEL}_${TIMESTAMP}.backup"
else
    FILENAME="chronok_db_${TIMESTAMP}.backup"
fi
FILEPATH="$BACKUP_DIR/$FILENAME"

if [ -z "${CHRONOKRAFT_DB_PASSWORD:-}" ]; then
    read -s -p "Mot de passe PostgreSQL pour $DB_USER: " PGPASSWORD
    echo ""
    export PGPASSWORD
else
    export PGPASSWORD="$CHRONOKRAFT_DB_PASSWORD"
fi

echo "Sauvegarde de '$DB_NAME' vers $FILEPATH..."
pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -Fc -f "$FILEPATH"

unset PGPASSWORD

SIZE=$(du -h "$FILEPATH" | cut -f1)
echo "Sauvegarde réussie : $FILENAME ($SIZE)"
