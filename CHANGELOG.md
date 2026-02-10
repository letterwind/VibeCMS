# 更新日誌

本專案的所有重要變更都會記錄在此檔案中。

格式基於 [Keep a Changelog](https://keepachangelog.com/zh-TW/1.0.0/)，
版本號遵循 [Semantic Versioning](https://semver.org/lang/zh-TW/)。

## [Unreleased]

### 計劃中
- 多語系支援（i18n）
- 檔案管理系統
- 圖片裁切與編輯功能
- 文章版本控制
- 評論系統

## [1.0.0] - 2026-02-10

### 新增
- 完整的後台管理系統
- 使用者認證與授權機制
  - JWT Token 認證
  - 圖形驗證碼
  - 帳號鎖定機制（5 次失敗鎖定 30 分鐘）
  - 密碼過期強制變更（3 個月）
- 角色權限管理（RBAC）
  - 自訂角色階層等級
  - 細粒度 CRUD 權限控制
  - 即時權限生效
- 使用者管理
  - 帳號密碼規則驗證
  - 密碼加密儲存
  - 使用者角色分配
- 功能管理
  - 動態後台選單配置
  - Bootstrap Icons 支援
  - 新視窗/當前視窗開啟設定
- 文章分類管理
  - 最多 3 層分類結構
  - SEO 優化欄位
  - 自訂 URL Slug
- 文章管理
  - TinyMCE 富文本編輯器
  - 標籤系統
  - SEO 優化欄位
  - 無限制內容長度
- 網站設定
  - 全域 SEO 設定
  - Favicon 上傳
  - 頁首/頁尾 HTML 自訂
- UI/UX 功能
  - 側邊滑入面板編輯
  - 刪除確認對話框
  - 響應式設計（桌面/平板/手機）
  - Toast 通知系統
- 資料保護
  - 軟刪除機制
  - 超級管理員永久刪除權限
- 測試覆蓋
  - 單元測試
  - 屬性測試（Property-Based Testing）
  - 整合測試

### 技術堆疊
- 後端：ASP.NET Core 10.0 WebAPI
- 前端：Angular 20.0
- 資料庫：SQL Server + Entity Framework Core 10.0
- UI 框架：Bootstrap 5.3
- 認證：JWT Bearer Token
- 驗證：FluentValidation 11.3
- 測試：xUnit + FsCheck + Jasmine

### 文件
- 完整的 README.md
- 需求規格文件（requirements.md）
- 技術設計文件（design.md）
- 實作任務文件（tasks.md）
- 貢獻指南（CONTRIBUTING.md）
- MIT 授權條款（LICENSE）

[Unreleased]: https://github.com/letterwind/VibeCMS/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/letterwind/VibeCMS/releases/tag/v1.0.0
