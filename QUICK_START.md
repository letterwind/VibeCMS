# 🚀 多語言系統 - 立即開始

## 30 秒快速開始

### **後端啟動**
```bash
cd src/WebCMS.Api
dotnet run
```
✅ API 運行在 `http://localhost:5000`

### **前端啟動**
```bash
cd src/WebCMS.Web
npm start
```
✅ 前端運行在 `http://localhost:4200`

### **訪問系統**
- 中文: http://localhost:4200/**zh-TW**/admin
- 英文: http://localhost:4200/**en-US**/admin  
- 日文: http://localhost:4200/**ja-JP**/admin

---

## 📝 核心功能演示

### 1️⃣ 查看文章 (自動語言過濾)

```
http://localhost:4200/zh-TW/admin/articles
↓
如果客戶端是中文: 自動顯示 languageCode='zh-TW' 的文章
```

### 2️⃣ 編輯多語言文章

```
http://localhost:4200/:lang/admin/articles/:id/edit

✅ 顯示所有語言標籤
✅ 顯示翻譯完成度 (✓ = 已翻譯, - = 未翻譯)
✅ 可在標籤間切換編輯
✅ 支持複製翻譯 (一鍵複製內容到其他語言)
✅ 支持刪除版本
```

### 3️⃣ 改變用戶語言

```
語言選擇器 (在導航欄)
↓
選擇 en-US
↓
URL 自動變為: http://localhost:4200/en-US/admin
↓
所有 API 請求自動添加: Header Accept-Language: en-US
```

### 4️⃣ 新建多語言內容

```
1. 以默認語言 (zh-TW) 創建內容
2. 打開編輯器
3. 複製到 en-US
4. 翻譯英文版本
5. 複製到 ja-JP
6. 翻譯日文版本
```

---

## 🔗 實用 URL 

| 功能           | URL                                         | 方法 |
| -------------- | ------------------------------------------- | ---- |
| 列表文章(中文) | GET `/api/articles?lang=zh-TW`              | 自動 |
| 獲取文章(中文) | GET `/api/articles/1?lang=zh-TW`            | 自動 |
| 所有版本       | GET `/api/articles/1/translations`          | REST |
| 翻譯狀態       | GET `/api/articles/1/translations/status`   | REST |
| 複製翻譯       | POST `/api/articles/1/translations/copy`    | 按鈕 |
| 刪除版本       | DELETE `/api/articles/1/translations/ja-JP` | 按鈕 |
| 支持語言       | GET `/api/languages`                        | 自動 |

---

## 💡 關鍵特點

✅ **自動語言檢測** - 從 URL 或 localStorage  
✅ **Header 自動注入** - 所有 API 自動添加語言信息  
✅ **路由保護** - 無效語言自動重定向  
✅ **複合索引** - (Slug + LanguageCode) = 每語言唯一  
✅ **翻譯追蹤** - 實時顯示翻譯進度  
✅ **一鍵複製** - 快速複製內容到其他語言  
✅ **版本管理** - 獨立刪除語言版本  
✅ **성능優化** - 後端 60 分鐘語言緩存  

---

## 📚 文檔

| 文檔                                 | 內容                    |
| ------------------------------------ | ----------------------- |
| **MULTI_LANGUAGE_IMPLEMENTATION.md** | 完整實現指南 (95% 完成) |
| **MULTI_LANGUAGE_TEST.md**           | 測試場景和驗證清單      |
| 本文件                               | 快速開始指南            |

---

## 🎬 演示場景

### **場景 A: 檢查文章翻譯狀態**

```bash
# 1. 打開瀏覽器控制台 (F12)
# 2. 訪問: http://localhost:4200/zh-TW/admin/articles/1/edit
# 3. 預期結果: 
#    - 顯示 3 個語言標籤
#    - zh-TW ✓ (已翻譯)
#    - en-US - (未翻譯)
#    - ja-JP - (未翻譯)
```

### **場景 B: 複製英文翻譯**

```bash
# 1. 確保在 /zh-TW/admin/articles/1/edit
# 2. 點擊「複製翻譯」按鈕
# 3. 選擇目標: en-US
# 4. 確認
# 預期:
#    - Network: POST /api/articles/1/translations/copy (200)
#    - UI: en-US 標籤變為 ✓
#    - 表單: 填充英文翻譯內容
```

### **場景 C: 測試路由保護**

```bash
# 1. 訪問: http://localhost:4200/invalid-lang/admin
# 預期: 重定向到 /zh-TW/admin

# 2. 訪問: http://localhost:4200/en-US/admin
# 預期: 保留 en-US，所有 API 使用 Accept-Language: en-US
```

### **場景 D: localStorage 持久性**

```bash
# 1. 切換到 en-US
# 2. 打開瀏覽器 DevTools → Application → localStorage
# 預期: preferredLanguage = "en-US"

# 3. 刷新頁面
# 預期: 仍然是 en-US (從 localStorage 恢復)
```

---

## 🛠️ 故障排除

### ❌ 問題: 訪問編輯器時出現 404

**解決方案**:
```bash
# 確保文章存在
GET http://localhost:5000/api/article/1

# 確保路由正確
# 應為: /zh-TW/admin/articles/:id/edit
# 不應為: /admin/articles/:id/edit
```

### ❌ 問題: 複製翻譯失敗 (400/500 錯誤)

**解決方案**:
```bash
# 檢查後端日誌
# 檢查源和目標語言是否有效
# 確保文章在源語言中存在

POST /api/articles/1/translations/copy
Body: { sourceLanguage: "zh-TW", targetLanguage: "en-US" }
```

### ❌ 問題: 語言未保存到 localStorage

**解決方案**:
```typescript
// 檢查 localStorage 是否被禁用
localStorage.getItem('preferredLanguage')  // 應返回語言代碼

// 檢查瀏覽器隱私模式（可能禁用 localStorage）
// 使用 SessionStorage 替代
```

### ❌ 問題: API 請求未包含語言 Header

**解決方案**:
```bash
# 打開瀏覽器 DevTools → Network
# 檢查請求標頭
# 應包含: Accept-Language: zh-TW
# 應包含查詢: ?lang=zh-TW

# 如果缺失，檢查:
# 1. LanguageInterceptor 是否在 app.config.ts 中註冊
# 2. LanguageService 是否正確初始化
```

---

## ✨ 隱藏功能

### 🔧 Angular DevTools
```bash
# 查看 LanguageService 的 BehaviorSubject
# 打開 Angular DevTools (Chrome Extension)
# 導航到 Services
# 查看 LanguageService 的內部狀態
```

### 📊 性能監控
```bash
# 打開瀏覽器控制台
console.log(performance.timing)

# 檢查:
# - LanguageService 初始化時間
# - 第一個 API 請求時間
# - 語言檢測時間
```

### 🐛 調試日誌
```typescript
// 編輯 language.service.ts
constructor(private http: HttpClient) {
  console.log('LanguageService 初始化...');
  this.initializeLanguages();
}

// 編輯 article-multi-language-edit.component.ts
ngOnInit(): void {
  console.log('編輯器組件初始化');
  console.log('文章 ID:', this.articleId);
}
```

---

## 📈 後續增強（可選）

### 🎯 第 1 優先級 - 用戶體驗
- [ ] 添加 loading spinner (複製翻譯時)
- [ ] 添加 toast 通知 (保存成功/失敗)
- [ ] 添加翻譯進度百分比
- [ ] 快速鍵支持 (Ctrl+S 保存)

### 🎯 第 2 優先級 - 功能
- [ ] 批量複製翻譯 (一次複製到所有語言)
- [ ] 翻譯對比視圖
- [ ] 版本歷史 (查看編輯歷史)
- [ ] 自動翻譯集成 (Google Translate API)

### 🎯 第 3 優先級 - 管理
- [ ] 創建新語言 (UI)
- [ ] 刪除語言 (UI)
- [ ] 語言順序重排 (拖拽)
- [ ] 翻譯完成度報告

---

## 🎓 學習資源

### 後端
- ASP.NET Core Identity
- EF Core Migration
- Generic Services Pattern

### 前端
- Angular Standalone Components
- RxJS BehaviorSubject
- HTTP Interceptors
- Route Guards

### 架構
- 多語言設計模式
- 數據庫複合索引
- API 版本控制

---

## 📞 快速支持 Q&A

**Q: 如何在生產環境中運行?**
```bash
dotnet publish -c Release
ng build --configuration production
```

**Q: 如何備份多語言數據?**
```bash
# 導出 Language 表和所有內容表 (以 LanguageCode 分組)
```

**Q: 如何遷移現有單語言內容?**
```bash
# 運行遷移腳本添加 LanguageCode='zh-TW' 到所有記錄
UPDATE Articles SET LanguageCode = 'zh-TW' WHERE LanguageCode IS NULL
```

**Q: 支持超過 3 種語言嗎?**
```bash
# 是的！添加到:
# 1. ApplicationDbContext.OnModelCreating() 種子數據
# 2. 前端 SUPPORTED_LANGUAGES 數組
# 3. LanguageService LANGUAGE_NAMES 映射
```

---

## 🎉 恭喜！

你已經有一個完整的多語言 CMS 系統準備就緒！

**下一步:**
1. 🧪 運行測試場景 (MULTI_LANGUAGE_TEST.md)
2. 📖 閱讀完整文檔 (MULTI_LANGUAGE_IMPLEMENTATION.md)
3. 🚀 部署到生產環境

---

**最後更新**: 2026 年 2 月 12 日  
**版本**: 1.0 (95% 完成)  
**狀態**: ✅ 準備就緒
