@echo off
:: ================================================
:: run-server.bat
:: Chạy server DarkRift chỉ với 1 lệnh (tự build + chạy)
:: 
:: Dùng: double-click file này, hoặc gõ: run-server.bat
:: ================================================

setlocal enabledelayedexpansion

echo === DarkRift Runner (CMD version) ===

:: Chuyển về thư mục chứa file .bat
cd /d "%~dp0"

set "MODE=%~1"
if /I "%MODE%"=="nobuild" goto run_only

:: 1. Build plugin (rat nhanh vi incremental)
echo.
echo [1/2] Building DR_Sever...
dotnet build "DR_Sever\DR_Game.csproj" -c Debug --no-restore -v minimal

if errorlevel 1 (
    echo.
    echo Build that bai! Sua loi code roi chay lai.
    pause
    exit /b 1
)

echo Build thanh cong.
goto run_server

:run_only
echo.
echo [1/2] Bo qua build ^(dung: run-server.bat nobuild^).

:: 2. Chay DarkRift
:run_server
echo.
echo [2/2] Khoi dong DarkRift Server...

set "DEPLOY=..\Deploy Server"
set "EXE=%DEPLOY%\DarkRift.Server.Console.exe"

if not exist "%EXE%" (
    echo.
    echo ERROR: Khong tim thay %EXE%
    echo Hay build it nhat 1 lan truoc do.
    pause
    exit /b 1
)

cd /d "%DEPLOY%"
"%EXE%"

echo.
echo Server da dung.
pause
