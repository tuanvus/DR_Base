# ================================================
# run-server.ps1
# Chạy 1 lệnh duy nhất: Build plugin + Khởi động DarkRift Server
#
# Cách dùng:
#   Từ thư mục Runner:   .\run-server.ps1
#   Hoặc double-click file này (nếu đã gán PowerShell)
#
# Ưu điểm: Không cần gõ riêng lệnh build rồi mới chạy server.
# ================================================

param(
    [switch]$NoBuild,          # .\run-server.ps1 -NoBuild   => chỉ chạy server, không build
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$runnerRoot = $PSScriptRoot
$deployRoot = Join-Path $runnerRoot "..\Deploy Server"
$serverExe  = Join-Path $deployRoot "DarkRift.Server.Console.exe"

Write-Host "=== DarkRift Runner ===" -ForegroundColor Cyan

# 1. Build (trừ khi dùng -NoBuild)
if (-not $NoBuild) {
    Write-Host "`n[1/2] Building plugin (incremental - rất nhanh sau lần đầu)..." -ForegroundColor Yellow
    
    $buildArgs = @(
        "build",
        "DR_Sever\DR_Game.csproj",
        "-c", $Configuration,
        "--no-restore",
        "-v", "minimal"
    )
    
    & dotnet @buildArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nBuild FAILED! Sửa lỗi rồi chạy lại." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Build thành công." -ForegroundColor Green
} else {
    Write-Host "`n[1/2] Bỏ qua build (dùng -NoBuild)." -ForegroundColor DarkGray
}

# 2. Chạy server
if (-not (Test-Path $serverExe)) {
    Write-Host "`nERROR: Không tìm thấy DarkRift.Server.Console.exe tại:`n$serverExe" -ForegroundColor Red
    Write-Host "Hãy chắc chắn bạn đã build ít nhất 1 lần trước đó." -ForegroundColor Yellow
    exit 1
}

Write-Host "`n[2/2] Đang khởi động DarkRift Server..." -ForegroundColor Green
Write-Host "Đường dẫn: $serverExe`n" -ForegroundColor DarkGray

# Chạy server (blocking - server chạy đến khi tắt)
& $serverExe

Write-Host "`nServer đã dừng." -ForegroundColor Yellow
pause
