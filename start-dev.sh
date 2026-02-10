#!/bin/bash

# VibeCMS 開發環境啟動腳本（Linux/Mac）
# 此腳本會同時啟動後端 API 和前端應用程式

echo "========================================"
echo "  VibeCMS 開發環境啟動中..."
echo "========================================"
echo ""

# 檢查 .NET SDK
echo "檢查 .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ 未安裝 .NET SDK！請從 https://dotnet.microsoft.com/download 下載安裝"
    exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET SDK 版本: $DOTNET_VERSION"

# 檢查 Node.js
echo "檢查 Node.js..."
if ! command -v node &> /dev/null; then
    echo "❌ 未安裝 Node.js！請從 https://nodejs.org 下載安裝"
    exit 1
fi
NODE_VERSION=$(node --version)
echo "✅ Node.js 版本: $NODE_VERSION"
echo ""

# 啟動後端 API
echo "啟動後端 API..."
API_PATH="src/WebCMS.Api"
if [ -d "$API_PATH" ]; then
    cd "$API_PATH"
    gnome-terminal -- bash -c "echo '後端 API 啟動中...'; dotnet run; exec bash" 2>/dev/null || \
    xterm -e "echo '後端 API 啟動中...'; dotnet run; bash" 2>/dev/null || \
    osascript -e 'tell app "Terminal" to do script "cd \"'$(pwd)'\"; echo \"後端 API 啟動中...\"; dotnet run"' 2>/dev/null &
    cd ../..
    echo "✅ 後端 API 啟動中 (https://localhost:5001)"
else
    echo "❌ 找不到後端專案目錄: $API_PATH"
    exit 1
fi

# 等待 API 啟動
echo "等待 API 啟動..."
sleep 5

# 啟動前端應用程式
echo "啟動前端應用程式..."
WEB_PATH="src/WebCMS.Web"
if [ -d "$WEB_PATH" ]; then
    cd "$WEB_PATH"
    gnome-terminal -- bash -c "echo '前端應用程式啟動中...'; npm start; exec bash" 2>/dev/null || \
    xterm -e "echo '前端應用程式啟動中...'; npm start; bash" 2>/dev/null || \
    osascript -e 'tell app "Terminal" to do script "cd \"'$(pwd)'\"; echo \"前端應用程式啟動中...\"; npm start"' 2>/dev/null &
    cd ../..
    echo "✅ 前端應用程式啟動中 (http://localhost:4200)"
else
    echo "❌ 找不到前端專案目錄: $WEB_PATH"
    exit 1
fi

echo ""
echo "========================================"
echo "  開發環境啟動完成！"
echo "========================================"
echo ""
echo "📝 存取資訊:"
echo "   前端應用: http://localhost:4200"
echo "   後端 API: https://localhost:5001"
echo "   Swagger:  https://localhost:5001/swagger"
echo ""
echo "👤 預設帳號:"
echo "   帳號: Admin01"
echo "   密碼: Admin123"
echo ""
echo "⚠️  請記得在首次登入後變更預設密碼！"
echo ""
