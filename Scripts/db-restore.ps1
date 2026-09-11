# db-restore.ps1 — Restauration de la base ChronoKraft depuis une sauvegarde
#
# Usage:
#   .\db-restore.ps1                                    # liste interactive
#   .\db-restore.ps1 -File "chronokraft_test1_...backup" # fichier précis
#
# ATTENTION : action destructive — écrase entièrement la base actuelle.
# Une confirmation explicite est demandée avant toute exécution.

param(
    [string]$File = ""
)

$ErrorActionPreference = "Stop"

$DbHost = "localhost"
$DbPort = "5432"
$DbName = "chronok_db"
$DbUser = "chronok_adm"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackupDir = Join-Path $ScriptDir "..\backups"

if (-not (Test-Path $BackupDir)) {
    Write-Host "Aucun dossier de sauvegardes trouvé ($BackupDir)." -ForegroundColor Red
    exit 1
}

if (-not $File) {
    $Backups = Get-ChildItem -Path $BackupDir -Filter "*.backup" | Sort-Object LastWriteTime -Descending
    if ($Backups.Count -eq 0) {
        Write-Host "Aucune sauvegarde disponible dans $BackupDir." -ForegroundColor Red
        exit 1
    }
    Write-Host "Sauvegardes disponibles :"
    for ($i = 0; $i -lt $Backups.Count; $i++) {
        Write-Host "  [$i] $($Backups[$i].Name) — $($Backups[$i].LastWriteTime)"
    }
    $Selection = Read-Host "Numéro de la sauvegarde à restaurer"
    $File = $Backups[[int]$Selection].FullName
}
elseif (-not (Test-Path $File)) {
    $CandidatePath = Join-Path $BackupDir $File
    if (Test-Path $CandidatePath) {
        $File = $CandidatePath
    } else {
        Write-Host "Fichier introuvable : $File" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "ATTENTION : ceci va ECRASER completement la base '$DbName' avec le contenu de :" -ForegroundColor Yellow
Write-Host "   $File" -ForegroundColor Yellow
Write-Host ""
$Confirm = Read-Host "Taper 'oui' pour confirmer"
if ($Confirm -ne "oui") {
    Write-Host "Restauration annulee."
    exit 0
}

if (-not $env:CHRONOKRAFT_DB_PASSWORD) {
    $SecurePassword = Read-Host "Mot de passe PostgreSQL pour $DbUser" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    $env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
} else {
    $env:PGPASSWORD = $env:CHRONOKRAFT_DB_PASSWORD
}

try {
    Write-Host "Suppression et recreation du schema 'public'..."
    $DropSql = "DROP SCHEMA public CASCADE; CREATE SCHEMA public; GRANT ALL ON SCHEMA public TO $DbUser;"
    & psql -h $DbHost -p $DbPort -U $DbUser -d $DbName -c $DropSql

    if ($LASTEXITCODE -ne 0) {
        throw "Echec de la reinitialisation du schema (code $LASTEXITCODE)"
    }

    Write-Host "Restauration en cours depuis $File..."
    & pg_restore -h $DbHost -p $DbPort -U $DbUser -d $DbName --no-owner --role=$DbUser $File

    if ($LASTEXITCODE -ne 0) {
        throw "pg_restore a echoue avec le code de sortie $LASTEXITCODE"
    }

    Write-Host "Restauration reussie." -ForegroundColor Green
}
finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
