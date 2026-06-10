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

:: 1. Build all relevant projects (Common, Define, Dto, Enum, Share, DR_Game)
echo.
echo [1/2] Building projects...
dotnet build "DR_Sever\DR_Game.csproj" -c Debug --no-restore -v minimal
dotnet build "Common\DR.Common.csproj" -c Debug --no-restore -v minimal
dotnet build "Define\DR.Define.csproj" -c Debug --no-restore -v minimal
dotnet build "Dto\DR.Dto.csproj" -c Debug --no-restore -v minimal
dotnet build "Enum\DR.Enum.csproj" -c Debug --no-restore -v minimal
dotnet build "Share\DR.Share.csproj" -c Debug --no-restore -v minimal

if errorlevel 1 (
    echo.
    echo Build that bai! Sua loi code roi chay lai.
    pause
    exit /b 1
)

echo Build thanh cong.

:: Copy built DLLs to Deploy Server\Lib (where server expects them)
set "DEPLOY=..\Deploy Server"
set "LIB=%DEPLOY%\Lib"

echo.
echo [Copy] Copying DLLs to %LIB% ...
if not exist "%LIB%" mkdir "%LIB%"

:: DR_Game (main plugin)
copy /Y "DR_Sever\bin\Debug\DR_Sever.dll" "%LIB%\" >nul 2>&1
copy /Y "DR_Sever\bin\Debug\DR_Sever.pdb" "%LIB%\" >nul 2>&1

:: Common
copy /Y "Common\bin\Debug\DR.Common.dll" "%LIB%\" >nul 2>&1
copy /Y "Common\bin\Debug\DR.Common.pdb" "%LIB%\" >nul 2>&1

:: Define
copy /Y "Define\bin\Debug\DR.Define.dll" "%LIB%\" >nul 2>&1
copy /Y "Define\bin\Debug\DR.Define.pdb" "%LIB%\" >nul 2>&1

:: Dto
copy /Y "Dto\bin\Debug\DR.Dto.dll" "%LIB%\" >nul 2>&1
copy /Y "Dto\bin\Debug\DR.Dto.pdb" "%LIB%\" >nul 2>&1

:: Enum
copy /Y "Enum\bin\Debug\DR.Enum.dll" "%LIB%\" >nul 2>&1
copy /Y "Enum\bin\Debug\DR.Enum.pdb" "%LIB%\" >nul 2>&1
copy /Y "Enum\bin\Debug\Enum.dll" "%LIB%\" >nul 2>&1

:: Share
copy /Y "Share\bin\Debug\Share.dll" "%LIB%\" >nul 2>&1
copy /Y "Share\bin\Debug\Share.pdb" "%LIB%\" >nul 2>&1

echo Copy to Lib done.
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
