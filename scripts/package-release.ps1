# ==============================================================================
# LDIS Release Packaging Script
# Produces a clean, deterministic, standalone release package for LDIS v1.0.0
# ==============================================================================

[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$rootDir = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$slnPath = Join-Path $rootDir "LDIS.sln"
$appBinDir = Join-Path $rootDir "src\LDIS.App\bin\$Configuration"
$distDir = Join-Path $rootDir "dist"
$stageDir = Join-Path $distDir "LDIS-v$Version"
$zipPath = Join-Path $distDir "LDIS-v$Version.zip"

Write-Host "=========================================================" -ForegroundColor Cyan
Write-Host " LDIS Release Packaging - Version $Version ($Configuration)" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

# 1. Locate MSBuild
$msbuildCandidates = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
)

$msbuildPath = $null
foreach ($cand in $msbuildCandidates) {
    if (Test-Path $cand) {
        $msbuildPath = $cand
        break
    }
}

if (-not $msbuildPath) {
    throw "MSBuild.exe for .NET Framework 4.0/4.8 was not found in Windows directory."
}

Write-Host "[1/6] Building $Configuration configuration..." -ForegroundColor Yellow
$buildOutput = & $msbuildPath $slnPath "/p:Configuration=$Configuration" "/t:Rebuild" "/verbosity:minimal" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host $buildOutput
    throw "MSBuild failed with exit code $LASTEXITCODE"
}
Write-Host "      Build succeeded with 0 errors." -ForegroundColor Green

# 2. Clean and prepare staging directory
Write-Host "[2/6] Preparing clean staging directory..." -ForegroundColor Yellow
if (Test-Path $zipPath) {
    Remove-Item -Path $zipPath -Force
}
if (Test-Path $stageDir) {
    Remove-Item -Path $stageDir -Recurse -Force
}
New-Item -ItemType Directory -Path $stageDir -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir "x86") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $stageDir "x64") -Force | Out-Null

# 3. Explicit whitelist of runtime files to deploy
Write-Host "[3/6] Copying whitelisted runtime files..." -ForegroundColor Yellow

$whitelist = @(
    @{ Source = "LDIS.App.exe"; Dest = "LDIS.App.exe" },
    @{ Source = "LDIS.App.exe.config"; Dest = "LDIS.App.exe.config" },
    @{ Source = "LDIS.Core.dll"; Dest = "LDIS.Core.dll" },
    @{ Source = "System.Data.SQLite.dll"; Dest = "System.Data.SQLite.dll" },
    @{ Source = "x86\SQLite.Interop.dll"; Dest = "x86\SQLite.Interop.dll" },
    @{ Source = "x64\SQLite.Interop.dll"; Dest = "x64\SQLite.Interop.dll" }
)

foreach ($item in $whitelist) {
    $srcFile = Join-Path $appBinDir $item.Source
    $dstFile = Join-Path $stageDir $item.Dest

    if (-not (Test-Path $srcFile)) {
        throw "CRITICAL: Required runtime dependency missing: $srcFile"
    }

    Copy-Item -Path $srcFile -Destination $dstFile -Force
    Write-Host "      + $($item.Dest)" -ForegroundColor Gray
}

# 4. Generate README.txt in release staging directory
Write-Host "[4/6] Creating deployment README.txt..." -ForegroundColor Yellow
$readmeContent = @"
================================================================================
LDIS - Lightweight Desktop Inventory System v$Version
================================================================================

A lightweight, offline-first Windows desktop inventory system designed for
factory and warehouse inventory management.

SYSTEM REQUIREMENTS
-------------------
* Operating System:
  - Windows 7 SP1 (32-bit or 64-bit) with .NET Framework 4.8
  - Windows 8 / 8.1 / 10 / 11
* Prerequisites:
  - Microsoft .NET Framework 4.8 Runtime
  - Note: No Visual C++ Redistributable or external database server required.

INSTALLATION & USAGE
--------------------
1. Extract this folder to any convenient location (e.g. C:\LDIS or Desktop).
2. Double-click "LDIS.App.exe" to start the application.
3. Optional: Create a shortcut to "LDIS.App.exe" on your Desktop.

DATA STORAGE
------------
* Default Standard Mode:
  The database file is stored in your Windows user profile at:
  %LocalAppData%\LDIS\inventory.db

* Optional Portable Mode:
  If a folder named "database" is created next to "LDIS.App.exe", LDIS will
  automatically use ".\database\inventory.db", enabling complete portability
  on a USB flash drive.

DATABASE BACKUP
---------------
* To create a backup snapshot, click "Backup DB..." in the main application
  header and select a safe destination (such as a USB drive or secondary disk).
* Database backups are self-contained standard SQLite database files (.db).

CSV EXPORT
----------
* Product catalogues and transaction histories can be exported to CSV at any
  time using the "Export CSV..." buttons. Exported files are formatted in UTF-8
  with BOM for direct compatibility with Microsoft Excel.

SUPPORT & TROUBLESHOOTING
-------------------------
* If the application does not start on Windows 7 SP1, verify that
  Microsoft .NET Framework 4.8 is installed on the computer.
================================================================================
"@

[System.IO.File]::WriteAllText((Join-Path $stageDir "README.txt"), $readmeContent, [System.Text.Encoding]::ASCII)
Write-Host "      + README.txt" -ForegroundColor Gray

# 5. Verification
Write-Host "[5/6] Verifying staged release integrity..." -ForegroundColor Yellow
$requiredFiles = @(
    "LDIS.App.exe",
    "LDIS.App.exe.config",
    "LDIS.Core.dll",
    "System.Data.SQLite.dll",
    "x86\SQLite.Interop.dll",
    "x64\SQLite.Interop.dll",
    "README.txt"
)

foreach ($relPath in $requiredFiles) {
    $checkPath = Join-Path $stageDir $relPath
    if (-not (Test-Path $checkPath)) {
        throw "Verification failed: $relPath does not exist in staging."
    }
    $size = (Get-Item $checkPath).Length
    if ($size -le 0) {
        throw "Verification failed: $relPath is 0 bytes."
    }
}

# Verify NO forbidden development files leaked into staging
$forbiddenPatterns = @("*.pdb", "*.xml", "*.Tests.*", "*.db", "*.log")
foreach ($pat in $forbiddenPatterns) {
    $found = Get-ChildItem -Path $stageDir -Filter $pat -Recurse
    if ($found) {
        throw "Packaging error: forbidden development file found in staging: $($found[0].FullName)"
    }
}
Write-Host "      Integrity check PASSED. Zero forbidden development files found." -ForegroundColor Green

# 6. Create ZIP archive
Write-Host "[6/6] Creating release archive: $zipPath..." -ForegroundColor Yellow
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stageDir, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $false)

$zipInfo = Get-Item $zipPath
$stageFiles = Get-ChildItem -Path $stageDir -Recurse -File
$totalSize = ($stageFiles | Measure-Object -Property Length -Sum).Sum

Write-Host ""
Write-Host "=========================================================" -ForegroundColor Green
Write-Host " RELEASE PACKAGING COMPLETE" -ForegroundColor Green
Write-Host "=========================================================" -ForegroundColor Green
Write-Host " Staged Folder: $stageDir ($($stageFiles.Count) files, $([math]::Round($totalSize / 1MB, 2)) MB)"
Write-Host " ZIP Archive:   $zipPath ($([math]::Round($zipInfo.Length / 1MB, 2)) MB)"
Write-Host "=========================================================" -ForegroundColor Green
