@echo off

cd /d "%~dp0Deploy Server"
if not exist "DarkRift.Server.Console.exe" (
    echo ERROR: Cannot find DarkRift.Server.Console.exe
    pause
    exit /b 1
)

"DarkRift.Server.Console.exe"

echo.
echo Server stopped.
pause
