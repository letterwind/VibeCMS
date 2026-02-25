# 翻譯參數使用指南

## 功能說明

翻譯服務現在支援在翻譯文本中使用動態參數，讓你可以在翻譯內容中插入變數值。

## 使用方式

### 1. 數組格式參數（索引佔位符）

適用於簡單的順序參數替換。

#### 語言資源定義
```json
{
  "validation": {
    "maxLength": "最多 {0} 字元",
    "minLength": "至少 {0} 字元",
    "range": "請輸入 {0} 到 {1} 之間的數字"
  }
}
```

#### 在模板中使用
```html
<!-- 單個參數 -->
<p>{{ 'validation.maxLength' | translate:[200] }}</p>
<!-- 輸出：最多 200 字元 -->

<p>{{ 'validation.minLength' | translate:[5] }}</p>
<!-- 輸出：至少 5 字元 -->

<!-- 多個參數 -->
<p>{{ 'validation.range' | translate:[1, 100] }}</p>
<!-- 輸出：請輸入 1 到 100 之間的數字 -->
```

#### 在 TypeScript 中使用
```typescript
import { LanguageService } from './core/services/language.service';

constructor(private languageService: LanguageService) {}

getMessage() {
  // 單個參數
  const msg1 = this.languageService.getTranslation('validation.maxLength', [200]);
  // 輸出：最多 200 字元
  
  // 多個參數
  const msg2 = this.languageService.getTranslation('validation.range', [1, 100]);
  // 輸出：請輸入 1 到 100 之間的數字
}
```

### 2. 對象格式參數（命名佔位符）

適用於需要明確參數名稱的場景，可讀性更好。

#### 語言資源定義
```json
{
  "message": {
    "greeting": "你好，{name}！",
    "welcome": "歡迎 {username}，你有 {count} 條新訊息",
    "fileInfo": "檔案 {filename} 大小為 {size} MB"
  }
}
```

#### 在模板中使用
```html
<!-- 單個命名參數 -->
<p>{{ 'message.greeting' | translate:{name: userName} }}</p>
<!-- 輸出：你好，張三！ -->

<!-- 多個命名參數 -->
<p>{{ 'message.welcome' | translate:{username: currentUser, count: messageCount} }}</p>
<!-- 輸出：歡迎 張三，你有 5 條新訊息 -->

<!-- 使用組件屬性 -->
<p>{{ 'message.fileInfo' | translate:{filename: file.name, size: file.size} }}</p>
<!-- 輸出：檔案 document.pdf 大小為 2.5 MB -->
```

#### 在 TypeScript 中使用
```typescript
getMessage() {
  const msg = this.languageService.getTranslation('message.welcome', {
    username: '張三',
    count: 5
  });
  // 輸出：歡迎 張三，你有 5 條新訊息
}
```

## 實際應用範例

### 表單驗證訊息

```typescript
// article-form.component.ts
export class ArticleFormComponent {
  maxTitleLength = 200;
  maxContentLength = 5000;
  
  getValidationMessage(field: string): string {
    if (field === 'title') {
      return this.languageService.getTranslation('validation.maxLength', [this.maxTitleLength]);
    }
    return '';
  }
}
```

```html
<!-- article-form.component.html -->
<mat-form-field>
  <mat-label>{{ 'article.title' | translate }}</mat-label>
  <input matInput formControlName="title" [maxlength]="maxTitleLength">
  <mat-hint>{{ 'validation.maxLength' | translate:[maxTitleLength] }}</mat-hint>
</mat-form-field>

<mat-form-field>
  <mat-label>{{ 'article.content' | translate }}</mat-label>
  <textarea matInput formControlName="content" [maxlength]="maxContentLength"></textarea>
  <mat-hint>{{ 'validation.maxLength' | translate:[maxContentLength] }}</mat-hint>
</mat-form-field>
```

### 動態訊息顯示

```html
<!-- 文章列表 -->
<div class="article-stats">
  <p>{{ 'article.stats' | translate:{total: articles.length, published: publishedCount} }}</p>
  <!-- 輸出：共 50 篇文章，已發布 30 篇 -->
</div>

<!-- 用戶資訊 -->
<div class="user-info">
  <h2>{{ 'user.profile' | translate:{name: user.name, role: user.role} }}</h2>
  <!-- 輸出：張三 - 管理員 -->
</div>
```

### 錯誤訊息

```json
{
  "error": {
    "fileSize": "檔案大小不能超過 {maxSize} MB",
    "fileType": "只允許上傳 {allowedTypes} 格式的檔案",
    "timeout": "操作超時（{seconds} 秒），請重試"
  }
}
```

```typescript
// 錯誤處理
showError(errorType: string, params: any) {
  const message = this.languageService.getTranslation(`error.${errorType}`, params);
  this.snackBar.open(message, '關閉', { duration: 3000 });
}

// 使用
this.showError('fileSize', { maxSize: 10 });
this.showError('fileType', { allowedTypes: 'JPG, PNG, PDF' });
this.showError('timeout', { seconds: 30 });
```

## 多語言支援

確保在所有語言檔案中使用相同的佔位符：

### zh-TW.json
```json
{
  "validation": {
    "maxLength": "最多 {0} 字元",
    "range": "請輸入 {min} 到 {max} 之間的數字"
  }
}
```

### en-US.json
```json
{
  "validation": {
    "maxLength": "Maximum {0} characters",
    "range": "Please enter a number between {min} and {max}"
  }
}
```

### ja-JP.json
```json
{
  "validation": {
    "maxLength": "最大 {0} 文字",
    "range": "{min} から {max} までの数字を入力してください"
  }
}
```

## 注意事項

1. 佔位符格式：
   - 數組參數使用 `{0}`, `{1}`, `{2}` 等
   - 對象參數使用 `{key}` 格式，key 必須是有效的變數名稱

2. 參數不存在時：
   - 如果參數索引超出範圍或 key 不存在，佔位符將保持原樣不被替換

3. 類型轉換：
   - 所有參數值都會被轉換為字串

4. 性能考量：
   - 參數替換在每次翻譯時執行，建議在組件中快取常用的翻譯結果

## 最佳實踐

1. 使用有意義的參數名稱（對象格式）
2. 在語言資源中添加註釋說明參數用途
3. 保持所有語言版本的佔位符一致
4. 對於複雜的格式化需求，考慮使用專門的格式化函數
