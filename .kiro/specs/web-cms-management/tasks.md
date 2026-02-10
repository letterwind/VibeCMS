# 實作計畫：Web CMS 管理系統

## 概述

本實作計畫將設計文件轉換為可執行的開發任務。採用漸進式開發方式，每個任務建立在前一個任務的基礎上，確保程式碼的完整性與可測試性。系統採用 ASP.NET Core 10 WebAPI 作為後端、Angular 20 作為前端框架，並使用 Bootstrap 5 實現響應式設計。

## 任務清單

- [x] 1. 建立專案結構與核心基礎設施
  - [x] 1.1 建立 ASP.NET Core 10 WebAPI 專案結構
    - 建立解決方案與專案目錄結構
    - 設定 Entity Framework Core 與資料庫連線
    - 建立基礎實體類別（BaseEntity）與軟刪除介面（ISoftDeletable）
    - _需求: 12.1, 12.3_
  
  - [x] 1.2 建立 Angular 20 前端專案結構
    - 使用 Angular CLI 建立專案
    - 設定 Bootstrap 5 與響應式佈局
    - 建立核心模組、共用模組、功能模組目錄結構
    - _需求: 11.5_
  
  - [x] 1.3 建立共用元件
    - 實作側邊滑入面板元件（SlidePanel）
    - 實作確認對話框元件（ConfirmDialog）
    - 實作資料表格元件（DataTable）
    - _需求: 11.1, 11.2_

- [x] 2. 實作認證模組
  - [x] 2.1 建立認證相關資料模型
    - 建立 AdminUser 實體與資料庫遷移
    - 建立 LoginAttempt 實體用於追蹤登入嘗試
    - 建立 LoginRequest、LoginResponse、CaptchaResponse DTOs
    - _需求: 1.1, 1.3_
  
  - [x] 2.2 實作驗證碼服務
    - 實作 ICaptchaService 介面
    - 產生圖形驗證碼並回傳 Base64 圖片
    - 實作驗證碼驗證邏輯（含 Token 機制）
    - _需求: 1.1, 1.5_
  
  - [x] 2.3 實作認證服務
    - 實作 IAuthService 介面
    - 實作帳號鎖定機制（5 次失敗鎖定 30 分鐘）
    - 實作自動解鎖機制（超過 30 分鐘自動解除）
    - 實作 JWT Token 產生與驗證
    - 實作密碼過期檢查（超過 3 個月強制變更）
    - _需求: 1.1, 1.3, 1.4, 1.6, 4.5_
  
  - [x] 2.4 撰寫認證模組屬性測試
    - **Property 1: 登入憑證驗證**
    - **Property 3: 驗證碼驗證**
    - **Property 4: 安全錯誤訊息**
    - **驗證: 需求 1.1, 1.5, 1.6**
  
  - [x] 2.5 實作認證控制器
    - 實作 AuthController（Login、Logout、GenerateCaptcha、ChangePassword）
    - 設定路由與 API 端點
    - _需求: 1.1, 4.6_
  
  - [x] 2.6 實作前端登入功能
    - 建立登入頁面元件
    - 實作驗證碼顯示與重新整理
    - 實作表單驗證與錯誤訊息顯示
    - 實作 JWT Token 儲存與 HTTP 攔截器
    - _需求: 1.1, 1.2, 1.5_

- [x] 3. 檢查點 - 確保認證模組測試通過
  - 確保所有測試通過，如有問題請詢問使用者。

- [x] 4. 實作密碼驗證服務
  - [x] 4.1 實作密碼驗證服務
    - 實作 IPasswordValidationService 介面
    - 實作帳號規則驗證（6-12 字元、至少 1 個大寫、1 個小寫、1 個數字）
    - 實作密碼規則驗證（同帳號規則）
    - 實作密碼不含帳號驗證
    - 實作密碼不重複驗證（新密碼不得與目前密碼相同）
    - _需求: 4.2, 4.3, 4.4, 4.6_
  
  - [x] 4.2 撰寫密碼驗證屬性測試
    - **Property 5: 憑證格式驗證**
    - **Property 6: 密碼不含帳號驗證**
    - **Property 7: 密碼變更不重複驗證**
    - **驗證: 需求 4.2, 4.3, 4.4, 4.6**

- [x] 5. 實作角色管理模組
  - [x] 5.1 建立角色相關資料模型
    - 建立 Role 實體與資料庫遷移
    - 建立 UserRole 關聯實體
    - 建立 RoleDto、CreateRoleRequest、UpdateRoleRequest DTOs
    - _需求: 2.1, 2.3_
  
  - [x] 5.2 實作角色服務
    - 實作 IRoleService 介面
    - 實作 CRUD 操作與軟刪除
    - 實作階層等級管理（支援自訂階層如 Admin < Manager < Finance < User）
    - _需求: 2.1, 2.3, 2.5_
  
  - [x] 5.3 撰寫角色管理屬性測試
    - **Property 8: 角色階層等級**
    - **Property 9: 軟刪除機制**
    - **驗證: 需求 2.3, 2.5**
  
  - [x] 5.4 實作角色控制器
    - 實作 RoleController（GetRoles、GetRole、CreateRole、UpdateRole、DeleteRole）
    - _需求: 2.1, 2.5_
  
  - [x] 5.5 實作前端角色管理功能
    - 建立角色列表頁面
    - 建立角色編輯側邊面板
    - 實作刪除確認對話框
    - _需求: 2.1, 2.2, 2.4, 2.5_

- [x] 6. 實作功能管理模組
  - [x] 6.1 建立功能相關資料模型
    - 建立 Function 實體與資料庫遷移
    - 建立 FunctionDto、CreateFunctionRequest、UpdateFunctionRequest DTOs
    - _需求: 5.1_
  
  - [x] 6.2 實作功能服務
    - 實作 IFunctionService 介面
    - 實作 CRUD 操作與軟刪除
    - 實作選單樹狀結構查詢
    - _需求: 5.1, 5.2, 5.6_
  
  - [x] 6.3 實作功能控制器
    - 實作 FunctionController（GetFunctions、GetFunction、CreateFunction、UpdateFunction、DeleteFunction、GetMenuTree）
    - _需求: 5.1, 5.6_
  
  - [x] 6.4 實作前端功能管理
    - 建立功能列表頁面
    - 建立功能編輯側邊面板（含 Bootstrap Icons 選擇器）
    - 實作連結開啟方式設定（新視窗/目前視窗）
    - _需求: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

- [x] 7. 實作權限管理模組
  - [x] 7.1 建立權限相關資料模型
    - 建立 RolePermission 實體與資料庫遷移
    - 建立 PermissionDto、SetPermissionsRequest、FunctionPermissionDto DTOs
    - _需求: 3.1_
  
  - [x] 7.2 實作權限服務
    - 實作 IPermissionService 介面
    - 實作權限設定與查詢（CRUD 四種權限獨立設定）
    - 實作權限檢查方法（HasPermissionAsync）
    - _需求: 3.1, 3.2, 3.3, 3.4_
  
  - [x] 7.3 撰寫權限管理屬性測試
    - **Property 10: CRUD 權限設定**
    - **Property 11: 權限存取控制**
    - **Property 12: 權限即時生效**
    - **驗證: 需求 3.1, 3.2, 3.3, 3.4**
  
  - [x] 7.4 實作權限控制器
    - 實作 PermissionController（GetPermissions、SetPermissions、GetFunctionPermissions）
    - _需求: 3.1, 3.2_
  
  - [x] 7.5 實作權限檢查中介軟體
    - 建立 PermissionAuthorizationHandler
    - 建立 PermissionAttribute
    - 實作權限不足時的錯誤回應
    - _需求: 3.3_
  
  - [x] 7.6 實作前端權限管理
    - 建立權限設定頁面（矩陣式 CRUD 勾選介面）
    - _需求: 3.1, 3.2_

- [x] 8. 檢查點 - 確保角色權限模組測試通過
  - 確保所有測試通過，如有問題請詢問使用者。

- [x] 9. 實作使用者管理模組
  - [x] 9.1 實作使用者服務
    - 實作 IUserService 介面
    - 整合密碼驗證服務
    - 實作 CRUD 操作與軟刪除
    - 實作密碼過期檢查
    - _需求: 4.1, 4.2, 4.3, 4.4, 4.5, 4.9_
  
  - [x] 9.2 實作使用者控制器
    - 實作 UserController（GetUsers、GetUser、CreateUser、UpdateUser、DeleteUser）
    - _需求: 4.1, 4.9_
  
  - [x] 9.3 實作前端使用者管理
    - 建立使用者列表頁面
    - 建立使用者編輯側邊面板
    - 實作表單驗證（帳號密碼規則即時驗證）
    - _需求: 4.1, 4.7, 4.8, 4.9_

- [x] 10. 實作文章分類管理模組
  - [x] 10.1 建立分類相關資料模型
    - 建立 Category 實體與資料庫遷移（含 Level 欄位）
    - 建立 CategoryDto、CreateCategoryRequest、UpdateCategoryRequest、CategoryTreeDto DTOs
    - _需求: 6.1, 6.2_
  
  - [x] 10.2 實作分類服務
    - 實作 ICategoryService 介面
    - 實作多層級分類（最多 3 層限制）
    - 實作分類深度檢查（GetCategoryDepth、CanAddChildCategory）
    - 實作級聯軟刪除（處理子分類及文章）
    - _需求: 6.1, 6.3, 6.6, 6.7_
  
  - [x] 10.3 撰寫分類管理屬性測試
    - **Property 13: 分類層級限制**
    - **Property 14: 分類名稱長度驗證**
    - **Property 15: 分類級聯處理**
    - **驗證: 需求 6.1, 6.3, 6.7**
  
  - [x] 10.4 實作分類控制器
    - 實作 CategoryController（GetCategories、GetCategory、CreateCategory、UpdateCategory、DeleteCategory、GetCategoryTree）
    - _需求: 6.1, 6.6_
  
  - [x] 10.5 實作前端分類管理
    - 建立分類列表頁面（樹狀結構顯示）
    - 建立分類編輯側邊面板（含 SEO 欄位：MetaTitle、MetaDescription、MetaKeywords）
    - 實作自訂網址代稱（Slug）輸入
    - _需求: 6.1, 6.2, 6.4, 6.5, 6.6_

- [x] 11. 實作文章管理模組
  - [x] 11.1 建立文章相關資料模型
    - 建立 Article 實體與資料庫遷移
    - 建立 Tag、ArticleTag 實體（多對多關聯）
    - 建立 ArticleDto、CreateArticleRequest、UpdateArticleRequest DTOs
    - _需求: 7.1_
  
  - [x] 11.2 實作文章服務
    - 實作 IArticleService 介面
    - 實作 CRUD 操作與軟刪除
    - 實作標籤管理（新增、關聯、移除）
    - _需求: 7.1, 7.2, 7.3, 7.4, 7.8_
  
  - [x] 11.3 撰寫文章管理屬性測試
    - **Property 16: 文章標題長度驗證**
    - **Property 17: 文章內容無限制**
    - **驗證: 需求 7.3, 7.4**
  
  - [x] 11.4 實作文章控制器
    - 實作 ArticleController（GetArticles、GetArticle、CreateArticle、UpdateArticle、DeleteArticle）
    - _需求: 7.1, 7.8_
  
  - [x] 11.5 實作前端文章管理
    - 建立文章列表頁面
    - 建立文章編輯側邊面板
    - 整合 TinyMCE HTML 編輯器
    - 實作標籤輸入元件
    - 實作 SEO 欄位（MetaTitle、MetaDescription、MetaKeywords）
    - 實作自訂網址代稱（Slug）輸入
    - _需求: 7.1, 7.5, 7.6, 7.7, 7.8_

- [x] 12. 檢查點 - 確保內容管理模組測試通過
  - 確保所有測試通過，如有問題請詢問使用者。

- [x] 13. 實作網站設定模組
  - [x] 13.1 建立設定相關資料模型
    - 建立 SiteSettings 實體與資料庫遷移（含 FaviconPath）
    - 建立 HeaderSettings 實體與資料庫遷移
    - 建立 FooterSettings 實體與資料庫遷移
    - 建立 SiteSettingsDto、HtmlSettingsDto、UpdateSiteSettingsRequest DTOs
    - _需求: 8.1, 9.1, 10.1_
  
  - [x] 13.2 實作設定服務
    - 實作 ISettingsService 介面
    - 實作單一記錄限制邏輯（不允許新增或刪除）
    - 實作 favicon 上傳處理
    - _需求: 8.1, 8.2, 8.3, 9.2, 10.2_
  
  - [x] 13.3 撰寫設定模組屬性測試
    - **Property 18: 單一記錄設定**
    - **驗證: 需求 8.3, 9.2, 10.2**
  
  - [x] 13.4 實作設定控制器
    - 實作 SiteSettingsController（GetSiteSettings、UpdateSiteSettings、UploadFavicon）
    - 實作 HeaderSettingsController（GetHeaderSettings、UpdateHeaderSettings）
    - 實作 FooterSettingsController（GetFooterSettings、UpdateFooterSettings）
    - _需求: 8.1, 8.4, 9.1, 9.3, 10.1, 10.3_
  
  - [x] 13.5 實作前端設定管理
    - 建立網站設定頁面（SEO 欄位、favicon 上傳）
    - 建立頁首設定頁面（HTML 編輯器）
    - 建立頁尾設定頁面（HTML 編輯器）
    - _需求: 8.1, 8.2, 8.4, 9.1, 9.3, 10.1, 10.3_

- [x] 14. 實作軟刪除與永久刪除功能
  - [x] 14.1 實作軟刪除基礎設施
    - 建立 ISoftDeletable 介面
    - 建立全域查詢篩選器（排除已刪除記錄）
    - 實作 IncludeDeleted 查詢選項
    - _需求: 11.3_
  
  - [x] 14.2 實作超級管理員永久刪除
    - 建立永久刪除 API 端點（HardDelete）
    - 實作超級管理員權限檢查
    - _需求: 11.4_
  
  - [x] 14.3 撰寫軟刪除屬性測試
    - **Property 9: 軟刪除機制**（整合測試）
    - **Property 19: 超級管理員永久刪除**
    - **驗證: 需求 11.3, 11.4**

- [x] 15. 實作必填欄位驗證
  - [x] 15.1 實作後端驗證
    - 建立 FluentValidation 驗證器（AccountValidator、PasswordValidator、CategoryNameValidator、ArticleTitleValidator）
    - 實作驗證錯誤回應格式（ErrorResponse）
    - _需求: 1.2, 2.2, 4.7, 5.4, 6.4, 7.6_
  
  - [x] 15.2 撰寫必填欄位驗證屬性測試
    - **Property 2: 必填欄位驗證**
    - **驗證: 需求 1.2, 2.2, 4.7, 5.4, 6.4, 7.6**
  
  - [x] 15.3 實作前端驗證
    - 建立 Angular 響應式表單驗證
    - 實作錯誤訊息顯示元件
    - _需求: 1.2, 2.2, 4.7, 5.4, 6.4, 7.6_

- [x] 16. 實作後台版面配置
  - [x] 16.1 建立版面配置元件
    - 建立 Header 元件（含使用者資訊、登出按鈕）
    - 建立 Sidebar 元件（動態選單，根據權限顯示）
    - 建立 Footer 元件
    - _需求: 5.1, 11.5_
  
  - [x] 16.2 實作響應式設計
    - 實作桌面版佈局
    - 實作平板版佈局
    - 實作手機版佈局（漢堡選單）
    - _需求: 11.5_

- [x] 17. 實作全域錯誤處理
  - [x] 17.1 實作後端錯誤處理
    - 建立 GlobalExceptionMiddleware
    - 定義錯誤代碼（ErrorCodes）
    - 實作各類例外處理（ValidationException、UnauthorizedException、ForbiddenException、NotFoundException、BusinessException）
    - _需求: 1.6, 3.3_
  
  - [x] 17.2 實作前端錯誤處理
    - 建立 ErrorInterceptor
    - 實作錯誤通知服務
    - 處理 401、403、404、422 等 HTTP 錯誤
    - _需求: 1.6, 3.3_

- [x] 18. 最終檢查點 - 確保所有測試通過
  - 確保所有測試通過，如有問題請詢問使用者。
  - 執行完整的整合測試
  - 驗證所有功能模組正常運作

## 備註

- 所有任務皆為必要任務，包含完整的測試覆蓋
- 每個任務都參照特定需求以確保可追溯性
- 檢查點確保漸進式驗證
- 屬性測試驗證通用正確性屬性（使用 FsCheck 後端、fast-check 前端）
- 單元測試驗證特定範例與邊界情況（使用 xUnit + FluentAssertions 後端、Jest 前端）
