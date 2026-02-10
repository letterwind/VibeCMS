# 貢獻指南

感謝您考慮為 VibeCMS 做出貢獻！

## 如何貢獻

### 回報問題

如果您發現 bug 或有功能建議：

1. 檢查 [Issues](https://github.com/letterwind/VibeCMS/issues) 確認問題尚未被回報
2. 建立新的 Issue，提供詳細資訊：
   - Bug 回報：重現步驟、預期行為、實際行為、環境資訊
   - 功能建議：使用情境、預期效果、可能的實作方式

### 提交程式碼

1. **Fork 專案**
   ```bash
   # 在 GitHub 上點擊 Fork 按鈕
   git clone https://github.com/YOUR_USERNAME/VibeCMS.git
   cd VibeCMS
   ```

2. **建立分支**
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   ```

3. **進行開發**
   - 遵循專案的程式碼風格
   - 撰寫清晰的 commit 訊息
   - 新增或更新測試
   - 更新相關文件

4. **測試**
   ```bash
   # 後端測試
   cd tests/WebCMS.Tests
   dotnet test
   
   # 前端測試
   cd src/WebCMS.Web
   npm test
   ```

5. **提交變更**
   ```bash
   git add .
   git commit -m "feat: add amazing feature"
   git push origin feature/your-feature-name
   ```

6. **建立 Pull Request**
   - 在 GitHub 上建立 Pull Request
   - 填寫 PR 模板
   - 等待審核

## 程式碼規範

### C# 後端

- 遵循 [Microsoft C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- 使用 4 個空格縮排
- 使用 PascalCase 命名類別和方法
- 使用 camelCase 命名私有欄位（前綴 `_`）
- 撰寫 XML 文件註解

```csharp
/// <summary>
/// 驗證使用者憑證
/// </summary>
/// <param name="account">使用者帳號</param>
/// <param name="password">使用者密碼</param>
/// <returns>驗證結果</returns>
public async Task<LoginResult> ValidateCredentials(string account, string password)
{
    // 實作內容
}
```

### TypeScript 前端

- 遵循 [Angular Style Guide](https://angular.io/guide/styleguide)
- 使用 2 個空格縮排
- 使用 camelCase 命名變數和方法
- 使用 PascalCase 命名類別和介面
- 撰寫 JSDoc 註解

```typescript
/**
 * 登入服務
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  /**
   * 使用者登入
   * @param credentials 登入憑證
   * @returns 登入回應
   */
  login(credentials: LoginCredentials): Observable<LoginResponse> {
    // 實作內容
  }
}
```

## Commit 訊息規範

使用 [Conventional Commits](https://www.conventionalcommits.org/) 格式：

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type 類型

- `feat`: 新功能
- `fix`: Bug 修復
- `docs`: 文件變更
- `style`: 程式碼格式（不影響功能）
- `refactor`: 重構（不是新功能也不是 bug 修復）
- `perf`: 效能改善
- `test`: 新增或修改測試
- `chore`: 建置流程或輔助工具變更

### 範例

```
feat(auth): add captcha validation

- Implement captcha generation service
- Add captcha validation in login flow
- Update login component with captcha input

Closes #123
```

## 分支策略

- `main`: 穩定的生產版本
- `develop`: 開發分支
- `feature/*`: 新功能分支
- `fix/*`: Bug 修復分支
- `hotfix/*`: 緊急修復分支

## 測試要求

所有新功能和 bug 修復都應該包含測試：

### 後端測試

- 單元測試：測試個別方法和類別
- 屬性測試：使用 FsCheck 測試通用屬性
- 整合測試：測試多個元件的互動

### 前端測試

- 單元測試：測試元件、服務、管道
- E2E 測試：測試完整的使用者流程

## 文件要求

- 更新 README.md（如果需要）
- 更新 API 文件（如果有 API 變更）
- 撰寫程式碼註解
- 更新 CHANGELOG.md

## 審核流程

1. 自動化測試必須通過
2. 至少一位維護者審核
3. 解決所有審核意見
4. 合併到目標分支

## 行為準則

- 尊重所有貢獻者
- 建設性的反饋
- 專注於問題本身，而非個人
- 歡迎新手貢獻

## 需要幫助？

- 查看 [Issues](https://github.com/letterwind/VibeCMS/issues) 中標記為 `good first issue` 的項目
- 在 Issue 中提問
- 查看專案文件

感謝您的貢獻！🎉
