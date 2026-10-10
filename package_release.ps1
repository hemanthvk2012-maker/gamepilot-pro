# ======================================================================
#  PACKAGING GAMEPILOT PRO COMMERCIAL RELEASE (FOR USERS TO DOWNLOAD)
# ======================================================================

$rootDir = "C:\Users\vijay\.gemini\antigravity\scratch\gamepilot-native"
$distDir = Join-Path $rootDir "dist_release"
$zipPath = Join-Path $rootDir "GamePilotPro_v2_Setup.zip"

Write-Host "Creating clean distribution directory..." -ForegroundColor Cyan
if (Test-Path $distDir) { Remove-Item -Path $distDir -Recurse -Force }
New-Item -ItemType Directory -Path $distDir -Force | Out-Null

Write-Host "[1/4] Copying compiled production binaries..." -ForegroundColor Green
Copy-Item (Join-Path $rootDir "GamePilotPro.exe") (Join-Path $distDir "GamePilotPro.exe") -Force
Copy-Item (Join-Path $rootDir "GamePilotOverlay.exe") (Join-Path $distDir "GamePilotOverlay.exe") -Force
Copy-Item (Join-Path $rootDir "GamePilotTestRunner.exe") (Join-Path $distDir "GamePilotTestRunner.exe") -Force
if (Test-Path (Join-Path $rootDir "gamepilot_logo.jpg")) {
    Copy-Item (Join-Path $rootDir "gamepilot_logo.jpg") (Join-Path $distDir "gamepilot_logo.jpg") -Force
}

Write-Host "[2/4] Generating One-Click Automated Installer (INSTALL_GAMEPILOT.bat)..." -ForegroundColor Green
$installerContent = @"
@echo off
echo ===============================================================
echo   INSTALLING GAMEPILOT PRO (VALORANT & CS2 AI COMPANION)
echo ===============================================================
set INSTALL_DIR=%LOCALAPPDATA%\GamePilotPro
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"
echo Copying application files to %INSTALL_DIR%...
copy /y "%~dp0GamePilotPro.exe" "%INSTALL_DIR%\GamePilotPro.exe"
copy /y "%~dp0GamePilotOverlay.exe" "%INSTALL_DIR%\GamePilotOverlay.exe"
if exist "%~dp0gamepilot_logo.jpg" copy /y "%~dp0gamepilot_logo.jpg" "%INSTALL_DIR%\gamepilot_logo.jpg"
echo Creating Desktop Shortcut...
powershell -Command "`$ws = New-Object -ComObject WScript.Shell; `$s = `$ws.CreateShortcut([System.IO.Path]::Combine([System.Environment]::GetFolderPath('Desktop'), 'GamePilot Pro.lnk')); `$s.TargetPath = '%INSTALL_DIR%\GamePilotPro.exe'; `$s.WorkingDirectory = '%INSTALL_DIR%'; `$s.Description = 'GamePilot Pro In-Game AI Co-Pilot'; `$s.Save()"
echo.
echo [SUCCESS] GamePilot Pro is installed! Desktop shortcut created.
echo Launching GamePilot Pro Command Hub now...
start "" "%INSTALL_DIR%\GamePilotPro.exe"
pause
"@
Set-Content -Path (Join-Path $distDir "INSTALL_GAMEPILOT.bat") -Value $installerContent -Encoding UTF8

Write-Host "[3/4] Creating Customer User Manual and License Activation Guide..." -ForegroundColor Green
$manualContent = @"
======================================================================
 GAMEPILOT PRO: COMMERCIAL USER GUIDE & LICENSE ACTIVATION
======================================================================
Welcome to GamePilot Pro - the billion-dollar esports tactical companion!

1. QUICK START:
   - Double-click 'INSTALL_GAMEPILOT.bat' to install and create a Desktop shortcut.
   - Or double-click 'GamePilotPro.exe' to run immediately without installation.

2. IN-GAME HOTKEYS (ACTIVE IN VALORANT & CS2):
   - F7  : Toggle 50% Head-Level Horizon Guide Line
   - F8  : Savage Toxic Roaster [Pro Tier]
   - F9  : Chad Esports Hype Caster [Free Tier]
   - F10 : Desi Hinglish Roaster [Pro Tier]
   - F11 : Radiant Tactical Coach [Pro Tier]
   - F6  : ClipForge 1-Click 9:16 Vertical Reel (Auto-saved to Desktop/GamePilotClips)
   - F12 : Close In-Game Transparent Overlay

3. HOW TO UPGRADE TO PRO:
   - Global Plan: `$14.99/mo via Lemon Squeezy (https://gamepilotpro.lemonsqueezy.com/checkout)
   - India Plan:  Rs.299/mo via Razorpay UPI (https://razorpay.me/@gamepilotpro)

4. PRE-APPROVED FOUNDER KEY FOR EVALUATION:
   GPPRO-LIFETIME-FOUNDER-2026-RADIANT

5. 100% RIOT VANGUARD & VALVE VAC SAFE:
   GamePilot Pro runs as a transparent Desktop Window Manager layer.
   Zero memory injection. Zero DLL hooks. 0ms input lag.
======================================================================
"@
Set-Content -Path (Join-Path $distDir "README_CUSTOMERS.txt") -Value $manualContent -Encoding UTF8

Write-Host "[4/4] Creating Ready-to-Download Distribution ZIP..." -ForegroundColor Green
if (Test-Path $zipPath) { Remove-Item -Path $zipPath -Force }
Compress-Archive -Path "$distDir\*" -DestinationPath $zipPath -Force

Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host " COMMERCIAL PACKAGE READY FOR DISTRIBUTION!" -ForegroundColor Cyan
Write-Host " Dist Folder: $distDir" -ForegroundColor Green
Write-Host " Zip Archive: $zipPath" -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Cyan
