using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Settings;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Settings;

/// <summary>
/// 設定模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class SettingsPropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    #region Property 18: 單一記錄設定

    /// <summary>
    /// Property 18: 單一記錄設定
    /// 對於網站設定、頁首設定、頁尾設定，系統應該限制為單一記錄，拒絕任何新增或刪除操作，僅允許更新。
    /// **Validates: Requirements 8.3, 9.2, 10.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_SiteSettings_ShouldOnlyHaveOneRecord(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);

        // Act - Ensure settings exist (should create exactly one record)
        service.EnsureSettingsExistAsync().Wait();
        var countAfterFirst = context.SiteSettings.Count();

        // Call ensure again (should not create another record)
        service.EnsureSettingsExistAsync().Wait();
        var countAfterSecond = context.SiteSettings.Count();

        // Assert - Should always have exactly one record
        return countAfterFirst == 1 && countAfterSecond == 1;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 頁首設定應該只有一筆記錄
    /// **Validates: Requirements 9.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_HeaderSettings_ShouldOnlyHaveOneRecord(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);

        // Act - Ensure settings exist
        service.EnsureSettingsExistAsync().Wait();
        var countAfterFirst = context.HeaderSettings.Count();

        // Call ensure multiple times
        for (int i = 0; i < seed.Get % 10 + 1; i++)
        {
            service.EnsureSettingsExistAsync().Wait();
        }
        var countAfterMultiple = context.HeaderSettings.Count();

        // Assert - Should always have exactly one record
        return countAfterFirst == 1 && countAfterMultiple == 1;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 頁尾設定應該只有一筆記錄
    /// **Validates: Requirements 10.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_FooterSettings_ShouldOnlyHaveOneRecord(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);

        // Act - Ensure settings exist
        service.EnsureSettingsExistAsync().Wait();
        var countAfterFirst = context.FooterSettings.Count();

        // Call ensure multiple times
        for (int i = 0; i < seed.Get % 10 + 1; i++)
        {
            service.EnsureSettingsExistAsync().Wait();
        }
        var countAfterMultiple = context.FooterSettings.Count();

        // Assert - Should always have exactly one record
        return countAfterFirst == 1 && countAfterMultiple == 1;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 服務應該標記為單一記錄限制
    /// **Validates: Requirements 8.3, 9.2, 10.2**
    /// </summary>
    [Fact]
    public void SingleRecord_Service_ShouldIndicateSingleRecordOnly()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);

        // Assert
        service.IsSingleRecordOnly.Should().BeTrue();
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 更新網站設定應該修改現有記錄而非新增
    /// **Validates: Requirements 8.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_UpdateSiteSettings_ShouldModifyExistingRecord(NonEmptyString title)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);
        service.EnsureSettingsExistAsync().Wait();

        var originalId = context.SiteSettings.First().Id;
        var countBefore = context.SiteSettings.Count();

        // Act - Update settings
        var request = new UpdateSiteSettingsRequest(title.Get, "Description", "Keywords");
        var result = service.UpdateSiteSettingsAsync(request).Result;

        var countAfter = context.SiteSettings.Count();
        var updatedId = context.SiteSettings.First().Id;

        // Assert - Should have same count and same ID
        return countBefore == 1 && countAfter == 1 && originalId == updatedId && result.MetaTitle == title.Get;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 更新頁首設定應該修改現有記錄而非新增
    /// **Validates: Requirements 9.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_UpdateHeaderSettings_ShouldModifyExistingRecord(NonEmptyString content)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);
        service.EnsureSettingsExistAsync().Wait();

        var originalId = context.HeaderSettings.First().Id;
        var countBefore = context.HeaderSettings.Count();

        // Act - Update settings
        var request = new UpdateHtmlSettingsRequest(content.Get);
        var result = service.UpdateHeaderSettingsAsync(request).Result;

        var countAfter = context.HeaderSettings.Count();
        var updatedId = context.HeaderSettings.First().Id;

        // Assert - Should have same count and same ID
        return countBefore == 1 && countAfter == 1 && originalId == updatedId && result.HtmlContent == content.Get;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 更新頁尾設定應該修改現有記錄而非新增
    /// **Validates: Requirements 10.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_UpdateFooterSettings_ShouldModifyExistingRecord(NonEmptyString content)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);
        service.EnsureSettingsExistAsync().Wait();

        var originalId = context.FooterSettings.First().Id;
        var countBefore = context.FooterSettings.Count();

        // Act - Update settings
        var request = new UpdateHtmlSettingsRequest(content.Get);
        var result = service.UpdateFooterSettingsAsync(request).Result;

        var countAfter = context.FooterSettings.Count();
        var updatedId = context.FooterSettings.First().Id;

        // Assert - Should have same count and same ID
        return countBefore == 1 && countAfter == 1 && originalId == updatedId && result.HtmlContent == content.Get;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 多次更新應該始終只有一筆記錄
    /// **Validates: Requirements 8.3, 9.2, 10.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SingleRecord_MultipleUpdates_ShouldMaintainSingleRecord(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);
        var random = new Random(seed.Get);

        // Act - Perform multiple updates
        var updateCount = random.Next(3, 10);
        for (int i = 0; i < updateCount; i++)
        {
            var siteRequest = new UpdateSiteSettingsRequest($"Title{i}", $"Desc{i}", $"Key{i}");
            service.UpdateSiteSettingsAsync(siteRequest).Wait();

            var headerRequest = new UpdateHtmlSettingsRequest($"<header>{i}</header>");
            service.UpdateHeaderSettingsAsync(headerRequest).Wait();

            var footerRequest = new UpdateHtmlSettingsRequest($"<footer>{i}</footer>");
            service.UpdateFooterSettingsAsync(footerRequest).Wait();
        }

        // Assert - Should still have exactly one record for each
        var siteCount = context.SiteSettings.Count();
        var headerCount = context.HeaderSettings.Count();
        var footerCount = context.FooterSettings.Count();

        return siteCount == 1 && headerCount == 1 && footerCount == 1;
    }

    /// <summary>
    /// Property 18: 單一記錄設定 - 取得設定應該自動建立預設記錄
    /// **Validates: Requirements 8.3, 9.2, 10.2**
    /// </summary>
    [Fact]
    public void SingleRecord_GetSettings_ShouldAutoCreateDefaultRecord()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new SettingsService(context);

        // Verify no records exist initially
        context.SiteSettings.Count().Should().Be(0);
        context.HeaderSettings.Count().Should().Be(0);
        context.FooterSettings.Count().Should().Be(0);

        // Act - Get settings (should auto-create)
        var siteSettings = service.GetSiteSettingsAsync().Result;
        var headerSettings = service.GetHeaderSettingsAsync().Result;
        var footerSettings = service.GetFooterSettingsAsync().Result;

        // Assert - Should have exactly one record for each
        context.SiteSettings.Count().Should().Be(1);
        context.HeaderSettings.Count().Should().Be(1);
        context.FooterSettings.Count().Should().Be(1);

        // Should return valid DTOs
        siteSettings.Should().NotBeNull();
        headerSettings.Should().NotBeNull();
        footerSettings.Should().NotBeNull();
    }

    #endregion
}
