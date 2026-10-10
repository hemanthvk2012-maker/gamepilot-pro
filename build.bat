@echo off
echo ======================================================================
echo  BUILDING GAMEPILOT PRO V2: COMMERCIAL PRODUCTION RELEASE
echo ======================================================================

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set SPEECH="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF\System.Speech.dll"
set WEBEXT="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Web.Extensions.dll"

echo Compiling GamePilotPro.exe...
%CSC% /optimize+ /target:winexe /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,%SPEECH%,%WEBEXT% /out:"%~dp0GamePilotPro.exe" "%~dp0src\Database.cs" "%~dp0src\LicenseManager.cs" "%~dp0src\ClipForge.cs" "%~dp0src\OverlayForm.cs" "%~dp0src\GamePilotHubForm.cs" "%~dp0src\Program.cs"

if %ERRORLEVEL% EQU 0 (
    copy /y "%~dp0GamePilotPro.exe" "%~dp0GamePilotOverlay.exe" > nul
    echo [SUCCESS] GamePilotPro.exe and GamePilotOverlay.exe built successfully!
    echo.
    echo Hotkeys in-game:
    echo   F6  = ClipForge 9:16 Vertical Reel
    echo   F7  = Toggle Head-Level Horizon
    echo   F8  = Savage Roaster Voice
    echo   F9  = Chad Hype Voice
    echo   F10 = Desi Hinglish Voice
    echo   F11 = Radiant Coach Voice
    echo   F12 = Exit Overlay
) else (
    echo [ERROR] Build failed. Check errors above.
)
pause
