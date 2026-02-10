# VibeCMS 開發環境啟動腳本
# 此腳本會同時啟動後端 API 和前端應用程式

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  VibeCMS 開發環境啟動中..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 檢查 .NET SDK
Write-Host "檢查 .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 未安裝 .NET SDK！請從 https://dotnet.microsoft.com/download 下載安裝" -ForegroundColor Red
    exit 1
}
Write-Host "✅ .NET SDK 版本: $dotnetVersion" -ForegroundColor Green

# 檢查 Node.js
Write-Host "檢查 Node.js..." -ForegroundColor Yellow
$nodeVersion = node --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 未安裝 Node.js！請從 https://nodejs.org 下載安裝" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Node.js 版本: $nodeVersion" -ForegroundColor Green
Write-Host ""

# 檢查資料庫連線
Write-Host "檢查資料庫連線..." -ForegroundColor Yellow
$connectionString = "Server=localhost;Database=VibeCMS;Trusted_Connection=True;TrustServerCertificate=True;"
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "✅ 資料庫連線成功" -ForegroundColor Green
} catch {
    Write-Host "⚠️  無法連線到資料庫，請確認 SQL Server 正在運行" -ForegroundColor Yellow
    Write-Host "   或使用 Docker: docker-compose up -d sqlserver" -ForegroundColor Yellow
}
Write-Host ""

# 啟動後端 API
Write-Host "啟動後端 API..." -ForegroundColor Yellow
$apiPath = "src\WebCMS.Api"
if (Test-Path $apiPath) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; Write-Host '後端 API 啟動中...' -ForegroundColor Cyan; dotnet run"
    Write-Host "✅ 後端 API 啟動中 (https://localhost:5001)" -ForegroundColor Green
} else {
    Write-Host "❌ 找不到後端專案目錄: $apiPath" -ForegroundColor Red
    exit 1
}

# 等待 API 啟動
Write-Host "等待 API 啟動..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# 啟動前端應用程式
Write-Host "啟動前端應用程式..." -ForegroundColor Yellow
$webPath = "src\WebCMS.Web"
if (Test-Path $webPath) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$webPath'; Write-Host '前端應用程式啟動中...' -ForegroundColor Cyan; npm start"
    Write-Host "✅ 前端應用程式啟動中 (http://localhost:4200)" -ForegroundColor Green
} else {
    Write-Host "❌ 找不到前端專案目錄: $webPath" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  開發環境啟動完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 存取資訊:" -ForegroundColor Yellow
Write-Host "   前端應用: http://localhost:4200" -ForegroundColor White
Write-Host "   後端 API: https://localhost:5001" -ForegroundColor White
Write-Host "   Swagger:  https://localhost:5001/swagger" -ForegroundColor White
Write-Host ""
Write-Host "👤 預設帳號:" -ForegroundColor Yellow
Write-Host "   帳號: Admin01" -ForegroundColor White
Write-Host "   密碼: Admin123" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  請記得在首次登入後變更預設密碼！" -ForegroundColor Red
Write-Host ""
Write-Host "按任意鍵關閉此視窗..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
