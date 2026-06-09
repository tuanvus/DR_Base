@echo off
echo ========================================
echo   Build MessagePack (Define + Dto)
echo   + Copy generated files to TestClient
echo ========================================

set SOLUTION_DIR=%~dp0
set DEFINE_PROJ=%SOLUTION_DIR%Runner\Define\DR.Define.csproj
set DTO_PROJ=%SOLUTION_DIR%Runner\Dto\DR.Dto.csproj
set TESTCLIENT_GEN=%SOLUTION_DIR%Runner\DR_TestClient\SerializationGeneration

echo.
echo [1/3] Restoring NuGet packages...
dotnet restore "%DEFINE_PROJ%"
dotnet restore "%DTO_PROJ%"

echo.
echo [2/3] Building DR.Define...
dotnet build "%DEFINE_PROJ%" -c Release
if errorlevel 1 goto :error

echo.
echo [3/3] Building DR.Dto (MessagePack Source Generator)...
dotnet build "%DTO_PROJ%" -c Release
if errorlevel 1 goto :error

echo.
echo [3/3] Copying generated MessagePack files...
if not exist "%TESTCLIENT_GEN%" mkdir "%TESTCLIENT_GEN%"

:: Copy from DR.Define
set GEN_SRC_DEFINE=%SOLUTION_DIR%Runner\Define\obj\Release\netstandard2.0\Generated\msgpack\Formatters
if exist "%GEN_SRC_DEFINE%" (
    xcopy /Y /E "%GEN_SRC_DEFINE%\*.g.cs" "%TESTCLIENT_GEN%\" >nul 2>&1
    echo   [OK] Copied from DR.Define
)

:: Copy from DR.Dto
set GEN_SRC_DTO=%SOLUTION_DIR%Runner\Dto\Generated\MessagePack.SourceGenerator\MessagePack.SourceGenerator.MessagePackGenerator
if exist "%GEN_SRC_DTO%" (
    xcopy /Y /E "%GEN_SRC_DTO%\*.g.cs" "%TESTCLIENT_GEN%\" >nul 2>&1
    echo   [OK] Copied from DR.Dto
) else (
    echo   [INFO] No generated files found (may need full rebuild)
)

echo.
echo ========================================
echo   DONE!
echo   Generated files copied to:
echo   Runner\DR_TestClient\SerializationGeneration\
echo ========================================
goto :end

:error
echo.
echo [ERROR] Build failed!
pause
exit /b 1

:end
pause
