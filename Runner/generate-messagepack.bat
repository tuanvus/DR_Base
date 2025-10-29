@echo off
setlocal enabledelayedexpansion
title MessagePack Code Generator
color 0B
cls

cd /d "%~dp0"

echo ╔════════════════════════════════════════╗
echo ║   MessagePack AOT Code Generator       ║
echo ╚════════════════════════════════════════╝
echo.

:: ========================================
:: CONFIG - BẠN CÓ SẴN MPC RỒI
:: ========================================

set MPC_EXE=C:\Users\ADMIN\.dotnet\tools\mpc.exe

:: ========================================
:: TÌM MSBUILD (RIDER OR VS 2022)
:: ========================================

echo [Step 1] Searching for MSBuild...

set MSBUILD_EXE=

:: Try 1: Rider MSBuild (priority)
for /d %%D in ("C:\Program Files\JetBrains\JetBrains Rider*") do (
    set TEST_PATH=%%D\tools\MSBuild\Current\Bin\MSBuild.exe
    if exist "!TEST_PATH!" (
        set MSBUILD_EXE=!TEST_PATH!
        echo   ✓ Found Rider MSBuild
        goto :found_msbuild
    )
)

:: Try 2: VS 2022 MSBuild
set VS_EDITIONS=Community Professional Enterprise
for %%E in (%VS_EDITIONS%) do (
    set TEST_PATH=C:\Program Files\Microsoft Visual Studio\2022\%%E\MSBuild\Current\Bin\MSBuild.exe
    if exist "!TEST_PATH!" (
        set MSBUILD_EXE=!TEST_PATH!
        echo   ✓ Found VS 2022 MSBuild (%%E)
        goto :found_msbuild
    )
)

echo   ✗ MSBuild not found!
echo   Please install Rider or VS 2022
pause
exit /b 1

:found_msbuild
echo   Path: %MSBUILD_EXE%
echo.

:: ========================================
:: GENERATE CODE
:: ========================================

echo [Step 2] Generating MessagePack code...
echo.

set SUCCESS=0
set FAILED=0

:: Define
echo [1/4] Define...
cd Define
if not exist "Serialization" mkdir Serialization
"%MPC_EXE%" -i "**/*.cs" -o "Serialization/DefineMessagePackGenerated.cs" -n "Define.Resolvers" -r "DefineMessagePackGenerated" -m "%MSBUILD_EXE%"
if exist "Serialization/DefineMessagePackGenerated.cs" (
    echo   ✓ Define OK
    set /a SUCCESS+=1
) else (
    echo   ✗ Define FAILED
    set /a FAILED+=1
)
cd ..

:: Common
echo [2/4] Common...
cd Common
if not exist "Serialization" mkdir Serialization
"%MPC_EXE%" -i "**/*.cs" -o "Serialization/CommonMessagePackGenerated.cs" -n "Common.Resolvers" -r "CommonMessagePackGenerated" -m "%MSBUILD_EXE%"
if exist "Serialization/CommonMessagePackGenerated.cs" (
    echo   ✓ Common OK
    set /a SUCCESS+=1
) else (
    echo   ✗ Common FAILED
    set /a FAILED+=1
)
cd ..

:: Share
echo [3/4] Share...
cd Share
if not exist "Serialization" mkdir Serialization
"%MPC_EXE%" -i "**/*.cs" -o "Serialization/ShareMessagePackGenerated.cs" -n "Share.Resolvers" -r "ShareMessagePackGenerated" -m "%MSBUILD_EXE%"
if exist "Serialization/ShareMessagePackGenerated.cs" (
    echo   ✓ Share OK
    set /a SUCCESS+=1
) else (
    echo   ✗ Share FAILED
    set /a FAILED+=1
)
cd ..

:: Enum
echo [4/4] Enum...
cd Enum
if not exist "Serialization" mkdir Serialization
"%MPC_EXE%" -i "**/*.cs" -o "Serialization/EnumMessagePackGenerated.cs" -n "Enum.Resolvers" -r "EnumMessagePackGenerated" -m "%MSBUILD_EXE%"
if exist "Serialization/EnumMessagePackGenerated.cs" (
    echo   ✓ Enum OK
    set /a SUCCESS+=1
) else (
    echo   ✗ Enum FAILED
    set /a FAILED+=1
)
cd ..

echo.
echo ╔════════════════════════════════════════╗
echo ║           Summary                      ║
echo ╠════════════════════════════════════════╣
echo ║  Total:   4                            ║
echo ║  Success: %SUCCESS%                            ║
echo ║  Failed:  %FAILED%                            ║
echo ╚════════════════════════════════════════╝
echo.

if %FAILED% gtr 0 (
    color 0C
    echo ERROR: Some projects failed to generate!
    echo.
    echo Possible reasons:
    echo   1. MSBuild incompatible with mpc
    echo   2. Missing MessagePackObject attributes
    echo   3. Syntax errors in source files
    echo.
    echo Try using Runtime MessagePack instead.
) else (
    color 0A
    echo SUCCESS! All projects generated successfully!
    echo.
    echo Next steps:
    echo   1. Add generated files to .csproj
    echo   2. Setup AOT resolver in code
    echo   3. Build solution
)

echo.
pause
