@echo off
echo ========================================
echo   Build Shared Libraries
echo   (Define + Dto + Enum + Share)
echo ========================================

set SOLUTION_DIR=%~dp0

echo.
echo [1/4] Building DR.Define...
dotnet build "%SOLUTION_DIR%Runner\Define\DR.Define.csproj" -c Release --no-restore
if errorlevel 1 goto :error

echo.
echo [2/4] Building DR.Dto...
dotnet build "%SOLUTION_DIR%Runner\Dto\DR.Dto.csproj" -c Release --no-restore
if errorlevel 1 goto :error

echo.
echo [3/4] Building DR.Enum...
dotnet build "%SOLUTION_DIR%Runner\Enum\DR.Enum.csproj" -c Release --no-restore
if errorlevel 1 goto :error

echo.
echo [4/4] Building DR.Share...
dotnet build "%SOLUTION_DIR%Runner\Share\DR.Share.csproj" -c Release --no-restore
if errorlevel 1 goto :error

echo.
echo ========================================
echo   All libraries built successfully!
echo   Output: Runner\*\bin\Release\*.dll
echo ========================================
goto :end

:error
echo.
echo [ERROR] Build failed!
pause
exit /b 1

:end
pause
