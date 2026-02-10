# VibeCMS - 網頁內容管理系統

VibeCMS 是一個功能完整的網頁內容管理系統（Web CMS），提供強大的後台管理功能，包含使用者認證、角色權限管理、文章管理、網站設定等核心功能。

## 技術堆疊

### 後端
- **框架**: ASP.NET Core 10.0 WebAPI (Controller-Based)
- **資料庫**: SQL Server
- **ORM**: Entity Framework Core 10.0
- **認證**: JWT Token (Bearer Authentication)
- **驗證**: FluentValidation 11.3
- **圖形處理**: SkiaSharp 3.119 (驗證碼生成)

### 前端
- **框架**: Angular 20.0
- **UI 框架**: Bootstrap 5.3
- **圖示**: Bootstrap Icons 1.11
- **HTML 編輯器**: TinyMCE 9.1
- **語言**: TypeScript 5.8

### 開發工具
- **.NET SDK**: 10.0 或更高版本
- **Node.js**: 18.x 或更高版本
- **npm**: 9.x 或更高版本

## 專案結構

```
VibeCMS/
├── src/
│   ├── WebCMS.Api/              # ASP.NET Core WebAPI 專案
│   ├── WebCMS.Core/             # 核心業務邏輯與領域模型
│   ├── WebCMS.Infrastructure/   # 資料存取與基礎設施
│   └── WebCMS.Web/              # Angular 前端專案
├── tests/
│   └── WebCMS.Tests/            # 單元測試與整合測試
└── .kiro/
    └── specs/                   # 專案規格文件
```

## 核心功能

### 後台管理
- ✅ **管理員登入** - 帳號密碼驗證、圖形驗證碼、帳號鎖定機制
- ✅ **角色管理** - 自訂角色階層等級（Admin < Manager < Finance < User）
- ✅ **權限管理** - 細粒度 CRUD 權限控制
- ✅ **使用者管理** - 帳號密碼規則驗證、密碼過期機制
- ✅ **功能管理** - 動態後台選單配置
- ✅ **文章分類管理** - 最多 3 層分類結構、SEO 優化
- ✅ **文章管理** - 富文本編輯器、標籤系統、SEO 優化
- ✅ **網站設定** - 全域 SEO、Favicon 上傳
- ✅ **頁首/頁尾設定** - HTML 自訂內容

### 特色功能
- 🔒 **安全性** - JWT 認證、密碼加密、帳號鎖定保護
- 🎨 **響應式設計** - 支援桌面、平板、手機裝置
- 🗑️ **軟刪除機制** - 資料保護，超級管理員可永久刪除
- 📝 **完整驗證** - 前後端雙重驗證，即時錯誤提示
- 🎯 **權限控制** - 基於角色的存取控制（RBAC）
- 🔌 **可擴充架構** - 模組化設計，預留電子商務擴充

## 快速開始

### 前置需求

1. 安裝 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
2. 安裝 [Node.js](https://nodejs.org/) (18.x 或更高版本)
3. 安裝 [SQL Server](https://www.microsoft.com/sql-server) 或 SQL Server Express

### 資料庫設定

1. 建立資料庫：
```sql
CREATE DATABASE VibeCMS;
```

2. 更新連線字串（`src/WebCMS.Api/appsettings.json`）：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VibeCMS;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. 執行資料庫遷移：
```bash
cd src/WebCMS.Api
dotnet ef database update
```

### 後端執行

1. 進入 API 專案目錄：
```bash
cd src/WebCMS.Api
```

2. 還原套件：
```bash
dotnet restore
```

3. 執行專案：
```bash
dotnet run
```

後端 API 將在 `https://localhost:5001` 啟動

### 前端執行

1. 進入前端專案目錄：
```bash
cd src/WebCMS.Web
```

2. 安裝相依套件：
```bash
npm install
```

3. 啟動開發伺服器：
```bash
npm start
```

前端應用程式將在 `http://localhost:4200` 啟動

### 預設管理員帳號

首次執行時，系統會自動建立預設管理員帳號：

- **帳號**: `Admin01`
- **密碼**: `Admin123`

> ⚠️ **重要**: 請在首次登入後立即變更預設密碼！

## 開發指南

### 後端開發

#### 建置專案
```bash
dotnet build
```

#### 執行測試
```bash
cd tests/WebCMS.Tests
dotnet test
```

#### 新增資料庫遷移
```bash
cd src/WebCMS.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

### 前端開發

#### 建置生產版本
```bash
cd src/WebCMS.Web
npm run build
```

#### 執行測試
```bash
npm test
```

#### 程式碼檢查
```bash
ng lint
```

## API 文件

後端 API 啟動後，可透過以下網址存取 Swagger 文件：

- **Swagger UI**: `https://localhost:5001/swagger`

## 部署

### 後端部署

1. 發布專案：
```bash
cd src/WebCMS.Api
dotnet publish -c Release -o ./publish
```

2. 設定生產環境的 `appsettings.Production.json`

3. 部署到 IIS、Azure App Service 或其他 ASP.NET Core 主機

### 前端部署

1. 建置生產版本：
```bash
cd src/WebCMS.Web
npm run build
```

2. 部署 `dist/webcms-web` 目錄到靜態網站主機（Nginx、Apache、Azure Static Web Apps 等）

## 環境變數

### 後端環境變數

| 變數名稱 | 說明 | 預設值 |
|---------|------|--------|
| `ConnectionStrings__DefaultConnection` | 資料庫連線字串 | - |
| `Jwt__Secret` | JWT 簽章密鑰 | - |
| `Jwt__Issuer` | JWT 發行者 | `VibeCMS` |
| `Jwt__Audience` | JWT 接收者 | `VibeCMS` |
| `Jwt__ExpirationMinutes` | Token 過期時間（分鐘） | `60` |

### 前端環境變數

編輯 `src/WebCMS.Web/src/environments/environment.ts`：

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api'
};
```

## 授權

本專案採用 MIT 授權條款。

## 貢獻

歡迎提交 Issue 或 Pull Request！

## 聯絡資訊

如有任何問題或建議，請透過 GitHub Issues 聯繫我們。

---

**VibeCMS** - 打造您的內容管理體驗 🚀
