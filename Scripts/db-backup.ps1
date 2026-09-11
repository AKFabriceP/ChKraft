# db-backup.ps1 — Sauvegarde de la base ChronoKraft
#
# Usage:
#   .\db-backup.ps1
#   .\db-backup.ps1 -Label "avant-migration-worker-skill"
#
# Le mot de passe est demandé de façon sécurisée, sauf si la variable
# d'environnement CHRONOKRAFT_DB_PASSWORD est déjà définie.

param(
    [string]$Label = ""
)

$ErrorActionPreference = "Stop"

$DbHost = "localhost"
$DbPort = "5432"
$DbName = "chronok_db"
$DbUser = "chronok_adm"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackupDir = Join-Path $ScriptDir "..\backups"
New-Item -ItemType Directory -Force -Path $BackupDir | Out-Null

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$LabelPart = if ($Label) { "_$Label" } else { "" }
$FileName = "chronok_db${LabelPart}_$Timestamp.backup"
$FilePath = Join-Path $BackupDir $FileName

if (-not $env:CHRONOKRAFT_DB_PASSWORD) {
    $SecurePassword = Read-Host "Mot de passe PostgreSQL pour $DbUser" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    $env:PGPASSWORD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
} else {
    $env:PGPASSWORD = $env:CHRONOKRAFT_DB_PASSWORD
}

Write-Host "Sauvegarde de '$DbName' vers $FilePath..."

try {
    & pg_dump -h $DbHost -p $DbPort -U $DbUser -d $DbName -Fc -f $FilePath

    if ($LASTEXITCODE -ne 0) {
        throw "pg_dump a échoué avec le code de sortie $LASTEXITCODE"
    }

    $SizeKB = [math]::Round((Get-Item $FilePath).Length / 1KB, 1)
    Write-Host "Sauvegarde réussie : $FileName ($SizeKB KB)" -ForegroundColor Green
}
finally {
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
