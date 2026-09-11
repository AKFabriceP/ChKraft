#!/bin/bash
# db-restore.sh — Restauration de la base ChronoKraft depuis une sauvegarde
#
# Usage:
#   ./db-restore.sh                                     # liste interactive
#   ./db-restore.sh "chronokraft_test1_....backup"       # fichier précis
#
# ATTENTION : action destructive — écrase entièrement la base actuelle.
# Une confirmation explicite est demandée avant toute exécution.

set -euo pipefail

DB_HOST="localhost"
DB_PORT="5432"
DB_NAME="chronok_db"
DB_USER="chronok_adm"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="$SCRIPT_DIR/../backups"

if [ ! -d "$BACKUP_DIR" ]; then
    echo "Aucun dossier de sauvegardes trouvé ($BACKUP_DIR)."
    exit 1
fi

FILE="${1:-}"

if [ -z "$FILE" ]; then
    mapfile -t BACKUPS < <(ls -t "$BACKUP_DIR"/*.backup 2>/dev/null || true)
    if [ ${#BACKUPS[@]} -eq 0 ]; then
        echo "Aucune sauvegarde disponible dans $BACKUP_DIR."
        exit 1
    fi
    echo "Sauvegardes disponibles :"
    for i in "${!BACKUPS[@]}"; do
        echo "  [$i] $(basename "${BACKUPS[$i]}")"
    done
    read -p "Numéro de la sauvegarde à restaurer: " SELECTION
    FILE="${BACKUPS[$SELECTION]}"
elif [ ! -f "$FILE" ]; then
    CANDIDATE="$BACKUP_DIR/$FILE"
    if [ -f "$CANDIDATE" ]; then
        FILE="$CANDIDATE"
    else
        echo "Fichier introuvable : $FILE"
        exit 1
    fi
fi

echo ""
echo "ATTENTION : ceci va ÉCRASER complètement la base '$DB_NAME' avec le contenu de :"
echo "   $FILE"
echo ""
read -p "Taper 'oui' pour confirmer: " CONFIRM
if [ "$CONFIRM" != "oui" ]; then
    echo "Restauration annulée."
    exit 0
fi

if [ -z "${CHRONOKRAFT_DB_PASSWORD:-}" ]; then
    read -s -p "Mot de passe PostgreSQL pour $DB_USER: " PGPASSWORD
    echo ""
    export PGPASSWORD
else
    export PGPASSWORD="$CHRONOKRAFT_DB_PASSWORD"
fi

echo "Suppression et recréation du schéma 'public'..."
psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c \
    "DROP SCHEMA public CASCADE; CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO $DB_USER;"

echo "Restauration en cours depuis $FILE..."
pg_restore -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" --no-owner --role="$DB_USER" "$FILE"

unset PGPASSWORD

echo "Restauration réussie."
