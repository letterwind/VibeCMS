# 需求文件

## 簡介

本文件定義了一個網頁內容管理系統（Web CMS）的需求規格。該系統提供完整的後台管理功能，包含使用者認證、角色權限管理、文章管理、網站設定等核心功能。系統採用 ASP.NET Core 10 WebAPI 作為後端、Angular 20 作為前端框架，並使用 Bootstrap 5 實現響應式設計，支援桌面、平板及手機裝置。

## 詞彙表

- **CMS_System**: 網頁內容管理系統的核心應用程式
- **Admin_User**: 具有後台管理權限的使用者
- **Role**: 定義使用者權限層級的角色實體
- **Permission**: 針對特定功能的存取權限設定
- **Function_Menu**: 後台選單功能項目
- **Article**: 文章內容實體
- **Category**: 文章分類實體
- **Site_Settings**: 網站全域設定
- **CAPTCHA**: 圖形驗證碼
- **Soft_Delete**: 軟刪除，標記為已刪除但保留資料
- **Super_Admin**: 超級管理員，具有最高權限
- **Slide_Panel**: 側邊滑入面板，用於編輯操作

## 需求

### 需求 1：管理員登入

**使用者故事：** 作為管理員，我希望能夠安全地登入後台系統，以便管理網站內容。

#### 驗收條件

1. WHEN 管理員輸入帳號、密碼及圖形驗證碼並提交 THEN THE CMS_System SHALL 驗證所有欄位並允許登入
2. WHEN 管理員未填寫任何必填欄位 THEN THE CMS_System SHALL 顯示該欄位的必填提示訊息
3. WHEN 管理員連續登入失敗達 5 次 THEN THE CMS_System SHALL 鎖定該帳號 30 分鐘
4. WHEN 帳號被鎖定超過 30 分鐘 THEN THE CMS_System SHALL 自動解除鎖定狀態
5. WHEN 圖形驗證碼輸入錯誤 THEN THE CMS_System SHALL 拒絕登入並重新產生驗證碼
6. IF 帳號或密碼錯誤 THEN THE CMS_System SHALL 顯示錯誤訊息但不透露具體錯誤原因

### 需求 2：角色管理

**使用者故事：** 作為超級管理員，我希望能夠管理系統角色，以便建立組織的權限層級架構。

#### 驗收條件

1. WHEN 管理員建立新角色 THEN THE CMS_System SHALL 要求輸入角色名稱、描述及階層等級
2. WHEN 管理員未填寫角色名稱 THEN THE CMS_System SHALL 顯示必填提示訊息
3. THE CMS_System SHALL 支援自訂階層等級（例如：Admin < Manager < Finance < User）
4. WHEN 管理員編輯角色 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單
5. WHEN 管理員刪除角色 THEN THE CMS_System SHALL 顯示確認對話框並執行軟刪除

### 需求 3：權限管理

**使用者故事：** 作為超級管理員，我希望能夠設定每個角色對各功能的存取權限，以便控制使用者的操作範圍。

#### 驗收條件

1. THE CMS_System SHALL 針對功能管理中的每個功能提供 CRUD 權限設定
2. WHEN 管理員設定角色權限 THEN THE CMS_System SHALL 允許個別設定新增、讀取、更新、刪除權限
3. WHEN 使用者嘗試存取無權限的功能 THEN THE CMS_System SHALL 拒絕存取並顯示權限不足訊息
4. WHEN 權限設定變更 THEN THE CMS_System SHALL 立即生效於該角色的所有使用者

### 需求 4：使用者管理

**使用者故事：** 作為管理員，我希望能夠管理系統使用者帳號，以便控制誰可以存取後台系統。

#### 驗收條件

1. WHEN 管理員建立使用者 THEN THE CMS_System SHALL 要求輸入帳號、密碼及顯示名稱
2. THE CMS_System SHALL 驗證帳號規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
3. THE CMS_System SHALL 驗證密碼規則：6-12 字元，必須包含至少 1 個大寫字母、1 個小寫字母、1 個數字
4. THE CMS_System SHALL 驗證密碼不得包含帳號內容
5. WHEN 密碼使用超過 3 個月 THEN THE CMS_System SHALL 強制使用者變更密碼
6. WHEN 使用者變更密碼 THEN THE CMS_System SHALL 驗證新密碼不得與目前密碼相同
7. WHEN 管理員未填寫任何必填欄位 THEN THE CMS_System SHALL 顯示該欄位的必填提示訊息
8. WHEN 管理員編輯使用者 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單
9. WHEN 管理員刪除使用者 THEN THE CMS_System SHALL 顯示確認對話框並執行軟刪除

### 需求 5：功能管理

**使用者故事：** 作為管理員，我希望能夠管理後台選單功能，以便自訂後台的導覽結構。

#### 驗收條件

1. WHEN 管理員建立功能項目 THEN THE CMS_System SHALL 要求輸入功能名稱、連結網址、開啟方式及圖示
2. THE CMS_System SHALL 支援設定連結在新視窗或目前視窗開啟
3. THE CMS_System SHALL 支援選擇 Bootstrap Icons 作為功能圖示
4. WHEN 管理員未填寫任何必填欄位 THEN THE CMS_System SHALL 顯示該欄位的必填提示訊息
5. WHEN 管理員編輯功能項目 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單
6. WHEN 管理員刪除功能項目 THEN THE CMS_System SHALL 顯示確認對話框並執行軟刪除

### 需求 6：文章分類管理

**使用者故事：** 作為內容編輯者，我希望能夠管理文章分類，以便組織網站內容結構。

#### 驗收條件

1. THE CMS_System SHALL 支援最多 3 層的多層級分類結構
2. WHEN 管理員建立分類 THEN THE CMS_System SHALL 要求輸入分類名稱、自訂網址代稱及 SEO 相關欄位
3. THE CMS_System SHALL 驗證分類名稱為必填且最多 20 字元
4. WHEN 管理員未填寫分類名稱 THEN THE CMS_System SHALL 顯示必填提示訊息
5. WHEN 管理員編輯分類 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單
6. WHEN 管理員刪除分類 THEN THE CMS_System SHALL 顯示確認對話框並執行軟刪除
7. WHEN 分類被刪除 THEN THE CMS_System SHALL 處理該分類下的子分類及文章

### 需求 7：文章管理

**使用者故事：** 作為內容編輯者，我希望能夠管理文章內容，以便發布和維護網站文章。

#### 驗收條件

1. WHEN 管理員建立文章 THEN THE CMS_System SHALL 要求輸入標題、內容、標籤、分類、自訂網址代稱及 SEO 相關欄位
2. THE CMS_System SHALL 驗證分類及標題為必填欄位
3. THE CMS_System SHALL 驗證標題最多 200 字元
4. THE CMS_System SHALL 支援無限制的內容長度並支援 HTML 格式
5. THE CMS_System SHALL 提供 HTML 編輯器（TinyMCE 或 CKEditor）用於編輯內容
6. WHEN 管理員未填寫任何必填欄位 THEN THE CMS_System SHALL 顯示該欄位的必填提示訊息
7. WHEN 管理員編輯文章 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單
8. WHEN 管理員刪除文章 THEN THE CMS_System SHALL 顯示確認對話框並執行軟刪除

### 需求 8：網站設定

**使用者故事：** 作為管理員，我希望能夠設定網站的全域設定，以便統一管理網站的基本資訊。

#### 驗收條件

1. THE CMS_System SHALL 提供全域 SEO 欄位設定
2. THE CMS_System SHALL 支援上傳網站 favicon
3. THE CMS_System SHALL 限制網站設定為單一記錄，不允許新增或刪除
4. WHEN 管理員編輯網站設定 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單

### 需求 9：全域頁首設定

**使用者故事：** 作為管理員，我希望能夠設定網站的全域頁首內容，以便在所有頁面顯示一致的頁首。

#### 驗收條件

1. THE CMS_System SHALL 提供 HTML 編輯器用於編輯頁首內容
2. THE CMS_System SHALL 限制頁首設定為單一記錄，不允許新增或刪除
3. WHEN 管理員編輯頁首設定 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單

### 需求 10：全域頁尾設定

**使用者故事：** 作為管理員，我希望能夠設定網站的全域頁尾內容，以便在所有頁面顯示一致的頁尾。

#### 驗收條件

1. THE CMS_System SHALL 提供 HTML 編輯器用於編輯頁尾內容
2. THE CMS_System SHALL 限制頁尾設定為單一記錄，不允許新增或刪除
3. WHEN 管理員編輯頁尾設定 THEN THE CMS_System SHALL 在側邊滑入面板中顯示編輯表單

### 需求 11：使用者介面與體驗

**使用者故事：** 作為使用者，我希望系統提供良好的使用體驗，以便高效地完成管理工作。

#### 驗收條件

1. THE CMS_System SHALL 使用側邊滑入面板方式顯示所有編輯視窗
2. WHEN 使用者執行刪除操作 THEN THE CMS_System SHALL 顯示確認對話框
3. THE CMS_System SHALL 預設使用軟刪除方式處理所有刪除操作
4. WHEN Super_Admin 存取已軟刪除的記錄 THEN THE CMS_System SHALL 允許永久刪除
5. THE CMS_System SHALL 支援桌面、平板及手機的響應式設計

### 需求 12：系統擴充性

**使用者故事：** 作為系統架構師，我希望系統具有良好的擴充性，以便未來能夠新增電子商務模組。

#### 驗收條件

1. THE CMS_System SHALL 採用模組化架構設計
2. THE CMS_System SHALL 提供清晰的 API 介面供未來模組擴充
3. THE CMS_System SHALL 使用依賴注入模式以支援模組替換
4. THE CMS_System SHALL 將核心功能與業務邏輯分離
