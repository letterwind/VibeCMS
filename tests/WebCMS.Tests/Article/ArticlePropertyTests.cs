using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Category;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Article;

/// <summary>
/// 文章管理模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class ArticlePropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task<int> CreateTestCategory(ApplicationDbContext context)
    {
        var categoryService = new CategoryService(context);
        var category = await categoryService.CreateCategoryAsync(new CreateCategoryRequest(
            "TestCategory",
            $"test-category-{Guid.NewGuid():N}",
            null,
            null,
            null,
            null,
            0
        ));
        return category.Id;
    }

    #region Property 16: 文章標題長度驗證

    /// <summary>
    /// Property 16: 文章標題長度驗證
    /// 對於任何超過 200 字元的文章標題，系統應該拒絕該文章建立或更新請求。
    /// **Validates: Requirements 7.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleTitle_ShouldRejectTitlesOver200Characters(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Generate a title longer than 200 characters
        var longTitle = new string('A', 201 + random.Next(100));

        // Act & Assert - Should throw exception for long title
        try
        {
            var request = new CreateArticleRequest(
                longTitle,
                "Test content",
                $"slug-{random.Next(10000)}",
                categoryId,
                null,
                null,
                null,
                null
            );
            articleService.CreateArticleAsync(request).Wait();
            return false; // Should not reach here
        }
        catch (AggregateException ex) when (ex.InnerException is ArgumentException)
        {
            return true; // Expected behavior
        }
    }

    /// <summary>
    /// Property 16: 文章標題長度驗證 - 200 字元以內的標題應該被接受
    /// **Validates: Requirements 7.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleTitle_ShouldAcceptTitlesUpTo200Characters(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Generate a title with 1-200 characters
        var titleLength = random.Next(1, 201);
        var validTitle = new string('B', titleLength);

        // Act
        var request = new CreateArticleRequest(
            validTitle,
            "Test content",
            $"slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var result = articleService.CreateArticleAsync(request).Result;

        // Assert
        return result.Title == validTitle && result.Title.Length <= 200;
    }

    /// <summary>
    /// Property 16: 文章標題長度驗證 - ValidateArticleTitle 方法應該正確驗證
    /// **Validates: Requirements 7.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleTitle_ValidateMethod_ShouldWorkCorrectly(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var random = new Random(seed.Get);

        // Test valid titles (1-200 characters)
        var validLength = random.Next(1, 201);
        var validTitle = new string('C', validLength);
        var validResult = articleService.ValidateArticleTitle(validTitle);

        // Test invalid titles (> 200 characters)
        var invalidLength = 201 + random.Next(100);
        var invalidTitle = new string('D', invalidLength);
        var invalidResult = articleService.ValidateArticleTitle(invalidTitle);

        // Test empty title
        var emptyResult = articleService.ValidateArticleTitle("");
        var nullResult = articleService.ValidateArticleTitle(null!);

        // Assert
        return validResult && !invalidResult && !emptyResult && !nullResult;
    }

    /// <summary>
    /// Property 16: 文章標題長度驗證 - 更新時也應該驗證標題長度
    /// **Validates: Requirements 7.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleTitle_UpdateShouldAlsoValidateLength(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Create a valid article first
        var createRequest = new CreateArticleRequest(
            "Valid Title",
            "Test content",
            $"slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var article = articleService.CreateArticleAsync(createRequest).Result;

        // Try to update with a title longer than 200 characters
        var longTitle = new string('E', 201 + random.Next(100));

        // Act & Assert
        try
        {
            var updateRequest = new UpdateArticleRequest(
                longTitle,
                "Updated content",
                article.Slug,
                categoryId,
                null,
                null,
                null,
                null
            );
            articleService.UpdateArticleAsync(article.Id, updateRequest).Wait();
            return false; // Should not reach here
        }
        catch (AggregateException ex) when (ex.InnerException is ArgumentException)
        {
            return true; // Expected behavior
        }
    }

    #endregion

    #region Property 17: 文章內容無限制

    /// <summary>
    /// Property 17: 文章內容無限制
    /// 對於任何長度的文章內容（包含 HTML 格式），系統應該能夠正確儲存並讀取，不受長度限制。
    /// **Validates: Requirements 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleContent_ShouldAcceptAnyLength(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Generate content of varying lengths (from 1 to 10000+ characters)
        var contentLength = random.Next(1, 10001);
        var content = new string('X', contentLength);

        // Act
        var request = new CreateArticleRequest(
            "Test Title",
            content,
            $"slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var result = articleService.CreateArticleAsync(request).Result;

        // Assert - Content should be stored and retrieved correctly
        return result.Content == content && result.Content.Length == contentLength;
    }

    /// <summary>
    /// Property 17: 文章內容無限制 - 應該支援 HTML 格式內容
    /// **Validates: Requirements 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleContent_ShouldSupportHtmlFormat(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Generate HTML content
        var htmlContent = $@"
            <html>
            <head><title>Test Article {random.Next(1000)}</title></head>
            <body>
                <h1>Article Heading</h1>
                <p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
                <ul>
                    <li>Item 1</li>
                    <li>Item 2</li>
                    <li>Item 3</li>
                </ul>
                <img src=""image.jpg"" alt=""Test Image"" />
                <a href=""https://example.com"">Link</a>
                <table>
                    <tr><td>Cell 1</td><td>Cell 2</td></tr>
                </table>
            </body>
            </html>";

        // Act
        var request = new CreateArticleRequest(
            "HTML Test Article",
            htmlContent,
            $"html-slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var result = articleService.CreateArticleAsync(request).Result;

        // Assert - HTML content should be preserved exactly
        return result.Content == htmlContent;
    }

    /// <summary>
    /// Property 17: 文章內容無限制 - 應該支援特殊字元
    /// **Validates: Requirements 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleContent_ShouldSupportSpecialCharacters(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Content with special characters
        var specialContent = $@"
            Special characters test {random.Next(1000)}:
            Unicode: 中文測試 日本語テスト 한국어테스트
            Symbols: © ® ™ € £ ¥ § ¶ † ‡ • ‰
            Math: ∑ ∏ ∫ ∂ √ ∞ ≈ ≠ ≤ ≥
            Arrows: ← → ↑ ↓ ↔ ⇐ ⇒ ⇑ ⇓
            Emojis: 😀 🎉 🚀 💻 📱
            HTML entities: &amp; &lt; &gt; &quot; &apos;
            Quotes: 'single' ""double"" «guillemets»
            Newlines and tabs:
            	Tab here
            Line break here";

        // Act
        var request = new CreateArticleRequest(
            "Special Characters Test",
            specialContent,
            $"special-slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var result = articleService.CreateArticleAsync(request).Result;

        // Assert - Special characters should be preserved
        return result.Content == specialContent;
    }

    /// <summary>
    /// Property 17: 文章內容無限制 - 更新時也應該支援任意長度內容
    /// **Validates: Requirements 7.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ArticleContent_UpdateShouldAlsoSupportAnyLength(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var articleService = new ArticleService(context);
        var categoryId = CreateTestCategory(context).Result;
        var random = new Random(seed.Get);

        // Create article with short content
        var createRequest = new CreateArticleRequest(
            "Test Title",
            "Short content",
            $"slug-{random.Next(100000)}",
            categoryId,
            null,
            null,
            null,
            null
        );
        var article = articleService.CreateArticleAsync(createRequest).Result;

        // Update with very long content
        var longContentLength = random.Next(5000, 10001);
        var longContent = new string('Y', longContentLength);

        var updateRequest = new UpdateArticleRequest(
            article.Title,
            longContent,
            article.Slug,
            categoryId,
            null,
            null,
            null,
            null
        );
        var updatedArticle = articleService.UpdateArticleAsync(article.Id, updateRequest).Result;

        // Assert
        return updatedArticle != null && 
               updatedArticle.Content == longContent && 
               updatedArticle.Content.Length == longContentLength;
    }

    #endregion
}
