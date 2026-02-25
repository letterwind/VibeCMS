/**
 * 多語言系統集成測試文件
 * 此文件提供驗證多語言功能的測試場景
 */

// ============ 1. 語言服務測試 ============
// 測試場景：語言初始化和切換
namespace LanguageServiceTest {
  // 預期行為：
  // 1. 服務應初始化並加載 3 種語言 (zh-TW, en-US, ja-JP)
  // 2. 默認語言應為 'zh-TW'
  // 3. 切換語言應更新 currentLanguage$ BehaviorSubject
  // 4. localStorage 應保存首選語言

  export const testCases = [
    {
      name: '初始化語言列表',
      steps: [
        '1. LanguageService 構造函數調用 initializeLanguages()',
        '2. HTTP GET /api/languages 應返回語言列表',
        '3. languages$ BehaviorSubject 應更新',
        '4. 如果 API 失敗，應使用硬編碼的默認語言'
      ],
      expectedResult: '3 種語言在 languages$ 中可用'
    },
    {
      name: '檢測和切換語言',
      steps: [
        '1. detectLanguage() 應檢查 URL > localStorage > 瀏覽器 > 默認',
        '2. setCurrentLanguage(code) 應驗證代碼',
        '3. 有效代碼應調用 currentLanguage$.next()',
        '4. localStorage.preferredLanguage 應更新'
      ],
      expectedResult: '語言正確切換並持久化'
    }
  ];
}

// ============ 2. 語言守衛測試 ============
// 測試場景：路由保護和語言驗證
namespace LanguageGuardTest {
  // 預期行為：
  // 1. 訪問 /zh-TW/admin 應通過（有效語言）
  // 2. 訪問 /xx-YY/admin 應重定向到 /zh-TW/admin（無效語言）
  // 3. 訪問無語言前綴的路由應自動添加前綴

  export const testCases = [
    {
      name: '有效語言代碼',
      url: '/zh-TW/admin/articles',
      expectedAction: '允許訪問，設置 currentLanguage$ 為 zh-TW'
    },
    {
      name: '無效語言代碼',
      url: '/invalid-lang/admin/articles',
      expectedAction: '重定向到 /zh-TW/admin/articles'
    },
    {
      name: '無語言前綴',
      url: '/admin/articles',
      expectedAction: '從 detectLanguage() 添加前綴'
    }
  ];
}

// ============ 3. HTTP 攔截器測試 ============
// 測試場景：自動語言注入
namespace LanguageInterceptorTest {
  // 預期行為：
  // 1. 所有 HTTP 請求應添加 Accept-Language header
  // 2. 所有請求應添加 lang query 參數
  // 3. 語言來自 languageService.getCurrentLanguageSync()

  export const testCases = [
    {
      name: '標頭注入',
      request: 'GET /api/articles',
      expectedHeaders: { 'Accept-Language': 'zh-TW' },
      expectedParams: { lang: 'zh-TW' }
    },
    {
      name: '切換語言後的請求',
      beforeSwitch: 'currentLanguage$ 設為 en-US',
      request: 'GET /api/articles',
      expectedHeaders: { 'Accept-Language': 'en-US' },
      expectedParams: { lang: 'en-US' }
    }
  ];
}

// ============ 4. 文章翻譯服務測試 ============
// 測試場景：多語言文章操作
namespace ArticleTranslationServiceTest {
  // 預期行為：
  // 1. getArticleTranslations(id) 應返回所有語言版本
  // 2. getArticleTranslationStatus(id) 應返回翻譯完成度
  // 3. copyArticleTranslation() 應從源語言複製到目標語言
  // 4. deleteLanguageVersion() 應軟刪除指定語言版本

  export const testCases = [
    {
      name: '加載所有翻譯版本',
      endpoint: 'GET /api/articles/1/translations',
      expectedResponse: '[ArticleDto(lang=zh-TW), ArticleDto(lang=en-US), ...]'
    },
    {
      name: '複製翻譯',
      endpoint: 'POST /api/articles/1/translations/copy',
      payload: { sourceLanguage: 'zh-TW', targetLanguage: 'en-US' },
      expectedResponse: 'ArticleDto(lang=en-US, 內容來自 zh-TW)'
    },
    {
      name: '刪除語言版本',
      endpoint: 'DELETE /api/articles/1/translations/en-US',
      expectedResponse: '成功刪除 (HTTP 204)'
    }
  ];
}

// ============ 5. 多語言編輯器組件測試 ============
// 測試場景：UI 交互
namespace ArticleMultiLanguageEditTest {
  // 預期行為：
  // 1. 打開編輯器時應加載所有語言版本
  // 2. 語言標籤應顯示翻譯狀態（✓/- 標記）
  // 3. 切換標籤應加載相應語言的內容
  // 4. 保存按鈕應調用 updateArticle() API
  // 5. 複製/刪除按鈕應如預期工作

  export const testCases = [
    {
      name: '初始化組件',
      steps: [
        'navigateTo("/zh-TW/admin/articles/1/edit")',
        'ngOnInit() 應調用 loadArticleTranslations()',
        'getArticleTranslations(1) 返回所有版本',
        'getArticleTranslationStatus(1) 顯示翻譯完成度'
      ],
      expectedResult: '語言標籤顯示已翻譯/未翻譯狀態'
    },
    {
      name: '編輯和保存',
      steps: [
        '修改標題和內容',
        '點擊「保存」',
        'updateArticle(id, request) 調用'
      ],
      expectedResult: 'API 返回更新後的 ArticleDto，本地 Map 更新'
    },
    {
      name: '複製翻譯',
      steps: [
        '點擊「複製翻譯」',
        '選擇目標語言 (en-US)',
        '確認對話框'
      ],
      expectedResult: 'copyArticleTranslation(1, zh-TW, en-US) 調用，新標籤顯示 ✓'
    }
  ];
}

// ============ 6. 後端 API 驗證 ============
// 測試場景：確保後端端點正確工作
namespace BackendAPITest {
  export const endpoints = [
    {
      method: 'GET',
      path: '/api/languages',
      description: '列表所有可用語言',
      expectedStatus: 200,
      expectedBody: '[{id,languageCode,languageName,isActive,sortOrder}, ...]'
    },
    {
      method: 'GET',
      path: '/api/articles?lang=zh-TW',
      description: '列出特定語言的文章',
      expectedStatus: 200,
      expectedBody: 'PagedResult<ArticleDto> with languageCode=zh-TW'
    },
    {
      method: 'GET',
      path: '/api/articles/1?lang=zh-TW',
      description: '獲取特定語言的文章',
      expectedStatus: 200,
      expectedBody: 'ArticleDto with languageCode=zh-TW'
    },
    {
      method: 'GET',
      path: '/api/articles/1/translations',
      description: '獲取所有語言版本',
      expectedStatus: 200,
      expectedBody: 'ArticleDto[]'
    },
    {
      method: 'GET',
      path: '/api/articles/1/translations/status',
      description: '獲取翻譯狀態',
      expectedStatus: 200,
      expectedBody: 'Dictionary<string, bool>'
    },
    {
      method: 'POST',
      path: '/api/articles/1/translations/copy',
      description: '複製翻譯',
      payload: { sourceLanguage: 'zh-TW', targetLanguage: 'en-US' },
      expectedStatus: 200,
      expectedBody: 'ArticleDto'
    },
    {
      method: 'DELETE',
      path: '/api/articles/1/translations/en-US',
      description: '刪除語言版本',
      expectedStatus: 204,
      expectedBody: 'null'
    }
  ];
}

// ============ 7. 端到端流程測試 ============
// 完整的多語言工作流程
namespace E2EWorkflow {
  export const scenario = `
    用例：編輯多語言文章

    前置條件：
    - 用戶已登錄
    - 文章 ID 1 已在 zh-TW 中存在

    步驟：
    1. 導航到 /zh-TW/admin/articles/1/edit
       ✓ LanguageGuard 驗證 lang 參數
       ✓ LanguageService.setCurrentLanguage('zh-TW')
       ✓ LanguageInterceptor 設置 Accept-Language header

    2. 組件加載
       ✓ loadArticleTranslations() 調用
       ✓ API: GET /api/articles/1/translations
         Header: Accept-Language: zh-TW, Query: lang=zh-TW
       ✓ 加載所有 3 種語言版本

    3. 加載翻譯狀態
       ✓ API: GET /api/articles/1/translations/status
       ✓ 顯示：zh-TW (✓), en-US (-), ja-JP (-)

    4. 編輯並保存中文版本
       ✓ 修改標題、內容
       ✓ 點擊「保存」
       ✓ API: PUT /api/articles/1
         Body: { title, content, ... }
         Header: Accept-Language: zh-TW
       ✓ 成功：本地 Map 更新

    5. 切換到英文標籤
       ✓ selectLanguage('en-US')
       ✓ LanguageService.setCurrentLanguage('en-US')
       ✓ 表單清空（因為 en-US 還沒有翻譯）

    6. 複製中文到英文
       ✓ 點擊「複製翻譯」
       ✓ 選擇 en-US，確認
       ✓ API: POST /api/articles/1/translations/copy
         Body: { sourceLanguage: 'zh-TW', targetLanguage: 'en-US' }
       ✓ 返回新的 ArticleDto (lang=en-US)
       ✓ en-US 標籤變為 ✓

    7. 編輯英文翻譯
       ✓ 翻譯標題和內容
       ✓ 保存
       ✓ API: PUT /api/articles/1
         Header: Accept-Language: en-US

    8. 刪除日本語版本
       ✓ 導航到 ja-JP 標籤
       ✓ 點擊「刪除版本」
       ✓ 確認刪除
       ✓ API: DELETE /api/articles/1/translations/ja-JP
       ✓ ja-JP 標籤變為 -

    結果：✅ 成功完成多語言編輯工作流程
  `;
}

// ============ 部署檢查清單 ============
export const DeploymentChecklist = {
  backend: [
    '✓ ApplicationDbContext 有 DbSet<Language>',
    '✓ Language 表已創建並填充 3 種語言',
    '✓ 所有內容實體有 LanguageCode 欄位',
    '✓ ILanguageService 和 LanguageService 已實現',
    '✓ ITranslationService<T> 和 TranslationService<T> 已實現',
    '✓ LanguageController 端點運作',
    '✓ ArticleTranslationController 端點運作',
    '✓ ArticleService 返回 LanguageCode 和 AvailableLanguages'
  ],
  frontend: [
    '✓ LanguageService 配置 with BehaviorSubject',
    '✓ LanguageGuard 實現為函數式 guard',
    '✓ LanguageInterceptor 實現為函數式 interceptor',
    '✓ app.config.ts 註冊 interceptor 和 guard',
    '✓ app.routes.ts 有 :lang 前綴',
    '✓ ArticleService 有多語言方法',
    '✓ LanguageSelectorComponent 已創建',
    '✓ ArticleMultiLanguageEditComponent 已創建',
    '✓ article-multi-language-edit 路由已添加'
  ],
  testing: [
    '[ ] 運行 `dotnet build` - 0 errors, 0 warnings',
    '[ ] 運行 Angular 類型檢查 - 0 errors',
    '[ ] 訪問 /zh-TW/admin - 應保留語言前綴',
    '[ ] 訪問 /invalid-lang/admin - 應重定向',
    '[ ] 刷新頁面 - localStorage 應恢復語言',
    '[ ] 打開瀏覽器控制台 - Accept-Language header 應存在',
    '[ ] 打開 /zh-TW/admin/articles/1/edit',
    '[ ] 驗證語言標籤和翻譯狀態顯示',
    '[ ] 編輯並保存 - API 應成功',
    '[ ] 複製翻譯 - API 應成功',
    '[ ] 刪除版本 - API 應成功'
  ]
};
