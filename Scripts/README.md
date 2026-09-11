# Scripts de Sauvegarde / Restauration — ChronoKraft

Scripts pour sauvegarder la base PostgreSQL locale à différents moments
(ex. avant un test risqué) et restaurer l'une de ces sauvegardes à la
demande.

**Note de validation :** les scripts Bash (`.sh`) ont été validés
syntaxiquement (`bash -n`). Les scripts PowerShell (`.ps1`) n'ont pas
pu être validés automatiquement (environnement sans `pwsh`) — à tester
lors de la première exécution.

## Prérequis

`pg_dump`, `pg_restore` et `psql` doivent être accessibles dans le
`PATH` — ils sont installés avec PostgreSQL (client ou serveur).

```bash
pg_dump --version   # confirme que les outils sont accessibles
```

## Sauvegarde

**Windows (PowerShell) :**
```powershell
.\db-backup.ps1
.\db-backup.ps1 -Label "avant-migration-worker-skill"
```

**Linux/macOS (Bash) :**
```bash
./db-backup.sh
./db-backup.sh "avant-migration-worker-skill"
```

Le fichier généré : `chronokraft_<label>_<horodatage>.backup`, dans
`../backups/` (relatif au dossier `scripts/`).

**Format utilisé :** `pg_dump -Fc` (format "custom" compressé) — plus
compact et plus rapide à restaurer qu'un export `.sql` brut, mais
nécessite `pg_restore` pour la restauration (pas `psql` directement).

## Restauration

⚠️ **Action destructive** — remplace entièrement le contenu actuel de
la base. Le script demande une confirmation explicite (taper `oui`)
avant d'exécuter quoi que ce soit.

**Windows (PowerShell) :**
```powershell
.\db-restore.ps1
# ou directement :
.\db-restore.ps1 -File "chronokraft_avant-migration-worker-skill_20260909-143000.backup"
```

**Linux/macOS (Bash) :**
```bash
./db-restore.sh
# ou directement :
./db-restore.sh "chronokraft_avant-migration-worker-skill_20260909-143000.backup"
```

Sans argument, le script liste les sauvegardes disponibles (les plus
récentes en premier) et demande laquelle restaurer.

## Mot de passe

Par défaut, le mot de passe PostgreSQL est demandé de façon masquée à
chaque exécution. Pour l'automatiser (ex. dans un script de test plus
large), définir la variable d'environnement avant d'appeler ces
scripts :

```bash
export CHRONOKRAFT_DB_PASSWORD="un_mot_de_passe_solide"   # Bash
$env:CHRONOKRAFT_DB_PASSWORD = "un_mot_de_passe_solide"    # PowerShell
```

**Ne jamais committer cette variable dans un script ou un fichier
versionné** — à définir uniquement dans l'environnement local ou un
gestionnaire de secrets.

## Ce que fait précisément la restauration

1. Supprime et recrée le schéma `public` (`DROP SCHEMA ... CASCADE`)
   — repart d'une base vide plutôt que de fusionner avec les données
   actuelles
2. Restaure le contenu du fichier `.backup` choisi via `pg_restore`

**Conséquence :** toute donnée présente dans la base *avant* la
restauration et absente de la sauvegarde choisie est perdue — c'est le
comportement voulu pour un usage de test (revenir à un état connu),
pas adapté à une fusion sélective de données.
