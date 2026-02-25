# WebCMS 多語言系統集成測試與使用指南

## 概覽

本指南說明如何測試和使用已實現的多語言架構，該架構支持：
- **文章 (Article)**
- **分類 (Category)**
- **功能 (Function)**
- **角色 (Role)**
- **系統 UI 資源 (Language Resources)**

---

## 1. 後端 API 端點概覽

### 1.1 語言資源管理 API

#### 獲取特定語言的所有資源
```
GET /api/language-resources/{languageCode}
Example: GET /api/language-resources/zh-TW
Response: { "success": true, "data": [...] }
```

#### 獲取單個資源
```
GET /api/language-resources/{languageCode}/{resourceKey}
Example: GET /api/language-resources/zh-TW/article.addArticle
```

#### 創建資源
```
POST /api/language-resources
Body: {
  "languageCode": "zh-TW",
  "resourceKey": "article.addArticle",
  "resourceValue": "新增文章",
  "resourceType": "UI",
  "description": "新增文章按鈕文字"
}
```

#### 導入 JSON 語言檔
```
POST /api/language-resources/{languageCode}/import
Body: {
  "fileContent": { ... },
  "overwrite": true
}
```

#### 導出為 JSON
```
GET /api/language-resources/{languageCode}/export
```

### 1.2 語言文件 API（前端使用）

#### 獲取語言 JSON 文件
```
GET /api/language-file/{languageCode}.json
Example: GET /api/language-file/zh-TW.json
Response: {
  "common": { "save": "保存", "cancel": "取消" },
  "article": { ... }
}
```

### 1.3 實體翻譯 API

#### 文章翻譯複製
```
POST /api/articles/{id}/translations/copy?sourceLanguage=zh-TW&targetLanguage=en-US
```

#### 分類翻譯複製
```
POST /api/categories/{id}/translations/copy?sourceLanguage=zh-TW&targetLanguage=en-US
```

#### 功能翻譯複製
```
POST /api/functions/{id}/translations/copy?sourceLanguage=zh-TW&targetLanguage=en-US
```

#### 角色翻譯複製
```
POST /api/roles/{id}/translations/copy?sourceLanguage=zh-TW&targetLanguage=en-US
```

---

## 2. 前端整合

### 2.1 使用 TranslatePipe 在模板中

```html
<!-- 按鍵翻譯（支持多層級） -->
<button>{{ 'button.addArticle' | translate }}</button>

<!-- 帶默認值 -->
<h1>{{ 'article.title' | translate: 'Article Title' }}</h1>
```

### 2.2 在服務中獲取翻譯

```typescript
import { LanguageService } from '../core/services/language.service';

constructor(private languageService: LanguageService) {}

// 獲取當前語言
const currentLang = this.languageService.getCurrentLanguageSync();

// 獲取翻譯字符串
const translation = this.languageService.getTranslation('article.addArticle');

// 訂閱語言資源
this.languageService.getLanguageResources().subscribe(resources => {
  console.log('Current language resources:', resources);
});
```

### 2.3 使用複製翻譯對話框

```html
<!-- 在模板中引入組件 -->
<app-copy-translation-dialog 
  [(isOpen)]="showCopyDialog"
  (copy)="onCopyTranslation($event)">
</app-copy-translation-dialog>

<!-- 觸發按鈕 -->
<button class="btn btn-sm" (click)="showCopyDialog = true">
  {{ 'translation.copyTranslation' | translate }}
</button>
```

```typescript
// 在組件類中處理複製事件
export class ArticleEditComponent {
  showCopyDialog = false;

  constructor(private articleService: ArticleService) {}

  onCopyTranslation(languages: { sourceLanguage: string; targetLanguage: string }): void {
    this.articleService.copyArticleTranslation(
      this.articleId,
      languages.sourceLanguage,
      languages.targetLanguage
    ).subscribe({
      next: (result) => {
        // 顯示成功消息
        console.log('複製成功:', result);
      },
      error: (error) => {
        // 顯示錯誤消息
        console.error('複製失敗:', error);
      }
    });
  }
}
```

---

## 3. 測試場景

### 3.1 語言選擇器測試

**操作步驟：**
1. 在應用程序中找到語言選擇器下拉菜單
2. 選擇不同的語言（繁體中文、英文、日文）
3. 驗證 UI 文本是否即時更新

**預期結果：**
- UI 文本應立即切換到選定語言
- 當前語言應保存到 localStorage
- 頁面刷新後仍保持該語言

### 3.2 文章翻譯複製測試

**操作步驟：**
1. 打開文章編輯頁面
2. 點擊「複製翻譯」按鈕
3. 選擇源語言（例如：繁體中文）和目標語言（例如：英文）
4. 點擊確認

**預期結果：**
- 應在目標語言創建新的文章版本
- 新版本應包含源語言的所有內容
- 用戶可以編輯目標語言版本

### 3.3 語言資源編輯器測試

**操作步驟：**
1. 打開「語言資源編輯器」（通常在管理後台）
2. 選擇一個語言（例如：繁體中文）
3. 在表格編輯模式中修改或新增資源
4. 點擊保存
5. 切換到 JSON 編輯模式，驗證變更

**預期結果：**
- 修改應立即保存到數據庫
- UI 應實時反映變更
- JSON 編輯模式和表格編輯模式應同步

### 3.4 兩源加載測試

**操作步驟：**
1. 啟動應用程序
2. 打開不同語言的頁面
3. 檢查瀏覽器控制台（F12）

**預期結果：**
- 應首先嘗試從 API (`/api/languageFile/{lang}.json`) 加載資源
- 如果 API 失敗，應回退到靜態文件 (`/assets/i18n/{lang}.json`)
- 資源應在內存中緩存 5 分鐘

---

## 4. API 測試示例（使用 curl 或 Postman）

### 4.1 獲取語言資源

```bash
# 獲取繁體中文資源
curl -X GET "http://localhost:5000/api/languageFile/zh-TW.json" \
  -H "Accept: application/json"

# 預期響應
{
  "common": {
    "save": "保存",
    "cancel": "取消",
    ...
  },
  "button": { ... },
  ...
}
```

### 4.2 導入新語言資源

```bash
curl -X POST "http://localhost:5000/api/languageResources/zh-TW/import" \
  -H "Content-Type: application/json" \
  -d '{
    "fileContent": {
      "newModule": {
        "action": "執行"
      }
    },
    "overwrite": false
  }'
```

### 4.3 複製文章翻譯

```bash
curl -X POST "http://localhost:5000/api/articles/1/translations/copy?sourceLanguage=zh-TW&targetLanguage=en-US" \
  -H "Content-Type: application/json" \
  -d '{}'
```

---

## 5. 快速開發指南

### 5.1 新增翻譯鍵

要為新功能添加翻譯：

1. **在語言文件中添加鍵**
   ```json
   // public/assets/i18n/zh-TW.json
   {
     "myModule": {
       "myKey": "我的翻譯文本"
     }
   }
   ```

2. **在模板中使用**
   ```html
   <p>{{ 'myModule.myKey' | translate }}</p>
   ```

3. **在服務中同步其他語言**
   - 更新 `en-US.json`、`ja-JP.json` 等

### 5.2 為新實體添加翻譯支持

1. **在數據庫中添加實體的 LanguageCode**
2. **更新 DTO 以包括 LanguageCode**
3. **在服務中添加翻譯方法**
   ```typescript
   copyEntityTranslation(id: Id, sourceLanguage: string, targetLanguage: string) {
     return this.api.post(`/api/entity/${id}/translations/copy?sourceLanguage=${sourceLanguage}&targetLanguage=${targetLanguage}`, {});
   }
   ```

---

## 6. 故障排除

### 問題：UI 文字未翻譯

**診斷步驟：**
1. 打開瀏覽器開發者工具（F12）
2. 檢查網絡標籤，查看 `/api/languageFile/{lang}.json` 請求是否成功
3. 在控制台中檢查是否有錯誤信息

**解決方案：**
- 確保語言資源已從 API 加載
- 檢查翻譯鍵是否拼寫正確（例如：`button.addArticle` 對比 `Button.AddArticle`）
- 清除瀏覽器快取並重新加載頁面

### 問題：複製翻譯失敗

**診斷步驟：**
1. 檢查後端控制台是否有錯誤
2. 驗證源和目標語言是否存在
3. 檢查實體是否存在於源語言

**解決方案：**
- 確保源語言實體已存在
- 驗證後端 API 端點是否正確
- 檢查數據庫連接和遷移狀態

### 問題：語言資源緩存未更新

**解決方案：**
- 點擊語言資源編輯器中的「清除快取」按鈕
- 或調用 API：`POST /api/languageFile/{languageCode}/clear-cache`
- 頁面刷新後應加載最新資源

---

## 7. 數據庫架構

### 7.1 LanguageResource 表

| 列名          | 類型          | 說明                       |
| ------------- | ------------- | -------------------------- |
| id            | int           | 主鍵                       |
| languageCode  | nvarchar(10)  | 語言代碼                   |
| resourceKey   | nvarchar(256) | 資源鍵（支持點記法）       |
| resourceValue | nvarchar(max) | 資源值                     |
| resourceType  | nvarchar(50)  | 資源類型（UI, Error, etc） |
| description   | nvarchar(256) | 描述                       |
| createdAt     | datetime2     | 創建時間                   |
| updatedAt     | datetime2     | 更新時間                   |
| createdBy     | nvarchar(100) | 創建者                     |
| updatedBy     | nvarchar(100) | 更新者                     |

**唯一索引：**(languageCode, resourceKey)

### 7.2 RolePermission 表（已更新）

| 列名         | 類型         | 說明                 |
| ------------ | ------------ | -------------------- |
| roleId       | int          | 角色 ID（複合主鍵）  |
| functionId   | int          | 功能 ID（複合主鍵）  |
| languageCode | nvarchar(10) | 語言代碼（複合主鍵） |
| createdAt    | datetime2    | 創建時間             |
| updatedAt    | datetime2    | 更新時間             |

**複合主鍵：**(roleId, functionId, languageCode)

---

## 8. 配置

### appsettings.json

```json
{
  "LanguageSettings": {
    "SupportedLanguages": ["zh-TW", "en-US", "ja-JP"],
    "DefaultLanguage": "zh-TW",
    "ResourceCacheDuration": 300,
    "AllowDatabaseResources": true,
    "AllowStaticResources": true,
    "StaticResourcePath": "assets/i18n"
  }
}
```

**配置說明：**
- `SupportedLanguages`: 支持的語言列表
- `DefaultLanguage`: 默認語言
- `ResourceCacheDuration`: 緩存時長（秒）
- `AllowDatabaseResources`: 是否從數據庫加載資源
- `AllowStaticResources`: 是否從靜態文件加載資源
- `StaticResourcePath`: 靜態文件路徑（相對於 wwwroot）

---

## 9. 性能注意事項

1. **語言資源緩存**：設置為 5 分鐘，降低數據庫查詢
2. **延遲加載**：資源按需加載，不會在應用啟動時全部加載
3. **兩源加載**：優先使用數據庫，可靠性更高
4. **Memory Cache**：使用 .NET 內存緩存，性能最優

---

## 10. 後續改進建議

1. **支持言語回退鏈**：例如 zh-CN 回退到 zh-TW
2. **實時語言文件編輯**：無需服務器重啟即可更新
3. **翻譯統計和覆蓋率報告**
4. **自動翻譯 API 集成**（如 Google Translate）
5. **i18n 鍵驗證工具**：檢測未翻譯或孤立的鍵

---

## 11. 聯繫和支持

如有問題或建議，請參考：
- 項目文檔：[MULTI_LANGUAGE_IMPLEMENTATION.md](../MULTI_LANGUAGE_IMPLEMENTATION.md)
- 測試用例：[MULTI_LANGUAGE_TEST.md](../MULTI_LANGUAGE_TEST.md)
- 項目完成總結：[PROJECT_COMPLETION_SUMMARY.md](../PROJECT_COMPLETION_SUMMARY.md)

---

**最後更新：2026 年 2 月 14 日**
