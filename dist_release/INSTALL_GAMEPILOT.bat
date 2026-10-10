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
powershell -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut([System.IO.Path]::Combine([System.Environment]::GetFolderPath('Desktop'), 'GamePilot Pro.lnk')); $s.TargetPath = '%INSTALL_DIR%\GamePilotPro.exe'; $s.WorkingDirectory = '%INSTALL_DIR%'; $s.Description = 'GamePilot Pro In-Game AI Co-Pilot'; $s.Save()"
echo.
echo [SUCCESS] GamePilot Pro is installed! Desktop shortcut created.
echo Launching GamePilot Pro Command Hub now...
start "" "%INSTALL_DIR%\GamePilotPro.exe"
pause
