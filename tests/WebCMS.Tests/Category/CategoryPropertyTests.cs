using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Common;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Category;

/// <summary>
/// 分類管理模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class CategoryPropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    #region Property 13: 分類層級限制

    /// <summary>
    /// Property 13: 分類層級限制
    /// 對於任何分類建立請求，若父分類已經是第 3 層，系統應該拒絕在其下建立子分類。
    /// **Validates: Requirements 6.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryDepth_ShouldRejectChildAtMaxDepth(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create a 3-level category hierarchy
        var level1Request = new CreateCategoryRequest(
            "Level1", $"level1-{random.Next(10000)}", null, null, null, null, 0);
        var level1 = service.CreateCategoryAsync(level1Request).Result;

        var level2Request = new CreateCategoryRequest(
            "Level2", $"level2-{random.Next(10000)}", level1.Id, null, null, null, 0);
        var level2 = service.CreateCategoryAsync(level2Request).Result;

        var level3Request = new CreateCategoryRequest(
            "Level3", $"level3-{random.Next(10000)}", level2.Id, null, null, null, 0);
        var level3 = service.CreateCategoryAsync(level3Request).Result;

        // Act - Try to create a 4th level category (should fail)
        var canAddChild = service.CanAddChildCategoryAsync(level3.Id).Result;

        // Assert - Should not be able to add child to level 3 category
        return !canAddChild && level3.Level == 3;
    }

    /// <summary>
    /// Property 13: 分類層級限制 - 可以在第 1 層和第 2 層下新增子分類
    /// **Validates: Requirements 6.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryDepth_ShouldAllowChildAtLowerLevels(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create level 1 category
        var level1Request = new CreateCategoryRequest(
            "Level1", $"level1-{random.Next(10000)}", null, null, null, null, 0);
        var level1 = service.CreateCategoryAsync(level1Request).Result;

        // Create level 2 category
        var level2Request = new CreateCategoryRequest(
            "Level2", $"level2-{random.Next(10000)}", level1.Id, null, null, null, 0);
        var level2 = service.CreateCategoryAsync(level2Request).Result;

        // Act - Check if we can add children to level 1 and level 2
        var canAddToLevel1 = service.CanAddChildCategoryAsync(level1.Id).Result;
        var canAddToLevel2 = service.CanAddChildCategoryAsync(level2.Id).Result;
        var canAddToRoot = service.CanAddChildCategoryAsync(null).Result;

        // Assert
        return canAddToRoot && canAddToLevel1 && canAddToLevel2;
    }

    /// <summary>
    /// Property 13: 分類層級限制 - 建立子分類時應該正確設定層級
    /// **Validates: Requirements 6.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryDepth_ShouldSetCorrectLevel(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create categories at each level
        var level1Request = new CreateCategoryRequest(
            "Level1", $"level1-{random.Next(10000)}", null, null, null, null, 0);
        var level1 = service.CreateCategoryAsync(level1Request).Result;

        var level2Request = new CreateCategoryRequest(
            "Level2", $"level2-{random.Next(10000)}", level1.Id, null, null, null, 0);
        var level2 = service.CreateCategoryAsync(level2Request).Result;

        var level3Request = new CreateCategoryRequest(
            "Level3", $"level3-{random.Next(10000)}", level2.Id, null, null, null, 0);
        var level3 = service.CreateCategoryAsync(level3Request).Result;

        // Assert - Each category should have the correct level
        return level1.Level == 1 && level2.Level == 2 && level3.Level == 3;
    }

    #endregion

    #region Property 14: 分類名稱長度驗證

    /// <summary>
    /// Property 14: 分類名稱長度驗證
    /// 對於任何超過 20 字元的分類名稱，系統應該拒絕該分類建立或更新請求。
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryName_ShouldRejectNamesOver20Characters(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Generate a name longer than 20 characters
        var longName = new string('A', 21 + random.Next(30));

        // Act & Assert - Should throw exception for long name
        try
        {
            var request = new CreateCategoryRequest(
                longName, $"slug-{random.Next(10000)}", null, null, null, null, 0);
            service.CreateCategoryAsync(request).Wait();
            return false; // Should not reach here
        }
        catch (AggregateException ex) when (ex.InnerException is ArgumentException)
        {
            return true; // Expected behavior
        }
    }

    /// <summary>
    /// Property 14: 分類名稱長度驗證 - 20 字元以內的名稱應該被接受
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryName_ShouldAcceptNamesUpTo20Characters(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Generate a name with 1-20 characters
        var nameLength = random.Next(1, 21);
        var validName = new string('A', nameLength);

        // Act
        var request = new CreateCategoryRequest(
            validName, $"slug-{random.Next(10000)}", null, null, null, null, 0);
        var result = service.CreateCategoryAsync(request).Result;

        // Assert
        return result.Name == validName && result.Name.Length <= 20;
    }

    /// <summary>
    /// Property 14: 分類名稱長度驗證 - ValidateCategoryName 方法應該正確驗證
    /// **Validates: Requirements 6.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryName_ValidateMethod_ShouldWorkCorrectly(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Test valid names (1-20 characters)
        var validLength = random.Next(1, 21);
        var validName = new string('B', validLength);
        var validResult = service.ValidateCategoryName(validName);

        // Test invalid names (> 20 characters)
        var invalidLength = 21 + random.Next(30);
        var invalidName = new string('C', invalidLength);
        var invalidResult = service.ValidateCategoryName(invalidName);

        // Test empty name
        var emptyResult = service.ValidateCategoryName("");
        var nullResult = service.ValidateCategoryName(null!);

        // Assert
        return validResult && !invalidResult && !emptyResult && !nullResult;
    }

    #endregion

    #region Property 15: 分類級聯處理

    /// <summary>
    /// Property 15: 分類級聯處理
    /// 對於任何被刪除的分類，系統應該同時處理該分類下的所有子分類，確保資料一致性。
    /// **Validates: Requirements 6.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryCascade_SoftDeleteShouldCascadeToChildren(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create a category hierarchy
        var level1Request = new CreateCategoryRequest(
            "Parent", $"parent-{random.Next(10000)}", null, null, null, null, 0);
        var level1 = service.CreateCategoryAsync(level1Request).Result;

        var level2Request = new CreateCategoryRequest(
            "Child1", $"child1-{random.Next(10000)}", level1.Id, null, null, null, 0);
        var level2 = service.CreateCategoryAsync(level2Request).Result;

        var level3Request = new CreateCategoryRequest(
            "Grandchild", $"grandchild-{random.Next(10000)}", level2.Id, null, null, null, 0);
        var level3 = service.CreateCategoryAsync(level3Request).Result;

        // Act - Soft delete the parent category
        var deleteResult = service.SoftDeleteCategoryAsync(level1.Id).Result;

        // Assert - All categories in the hierarchy should be soft deleted
        var parentDeleted = context.Categories
            .IgnoreQueryFilters()
            .First(c => c.Id == level1.Id);
        var childDeleted = context.Categories
            .IgnoreQueryFilters()
            .First(c => c.Id == level2.Id);
        var grandchildDeleted = context.Categories
            .IgnoreQueryFilters()
            .First(c => c.Id == level3.Id);

        return deleteResult &&
               parentDeleted.IsDeleted &&
               childDeleted.IsDeleted &&
               grandchildDeleted.IsDeleted;
    }

    /// <summary>
    /// Property 15: 分類級聯處理 - 軟刪除後子分類不應該在正常查詢中可見
    /// **Validates: Requirements 6.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryCascade_DeletedChildrenShouldNotBeVisible(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create parent with multiple children
        var parentRequest = new CreateCategoryRequest(
            "Parent", $"parent-{random.Next(10000)}", null, null, null, null, 0);
        var parent = service.CreateCategoryAsync(parentRequest).Result;

        var childCount = random.Next(2, 5);
        var childIds = new List<int>();
        for (int i = 0; i < childCount; i++)
        {
            var childRequest = new CreateCategoryRequest(
                $"Child{i}", $"child{i}-{random.Next(10000)}", parent.Id, null, null, null, 0);
            var child = service.CreateCategoryAsync(childRequest).Result;
            childIds.Add(child.Id);
        }

        // Get count before delete
        var countBefore = service.GetAllCategoriesAsync().Result.Count;

        // Act - Soft delete the parent
        service.SoftDeleteCategoryAsync(parent.Id).Wait();

        // Get count after delete
        var countAfter = service.GetAllCategoriesAsync().Result.Count;

        // Assert - All categories (parent + children) should be hidden
        return countBefore == childCount + 1 && countAfter == 0;
    }

    /// <summary>
    /// Property 15: 分類級聯處理 - 永久刪除應該級聯刪除所有子分類
    /// **Validates: Requirements 6.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryCascade_HardDeleteShouldRemoveAllChildren(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create a category hierarchy
        var level1Request = new CreateCategoryRequest(
            "Parent", $"parent-{random.Next(10000)}", null, null, null, null, 0);
        var level1 = service.CreateCategoryAsync(level1Request).Result;

        var level2Request = new CreateCategoryRequest(
            "Child", $"child-{random.Next(10000)}", level1.Id, null, null, null, 0);
        var level2 = service.CreateCategoryAsync(level2Request).Result;

        var level3Request = new CreateCategoryRequest(
            "Grandchild", $"grandchild-{random.Next(10000)}", level2.Id, null, null, null, 0);
        var level3 = service.CreateCategoryAsync(level3Request).Result;

        // Act - Hard delete the parent category
        var deleteResult = service.HardDeleteCategoryAsync(level1.Id).Result;

        // Assert - All categories should be permanently removed
        var remainingCount = context.Categories
            .IgnoreQueryFilters()
            .Count();

        return deleteResult && remainingCount == 0;
    }

    /// <summary>
    /// Property 15: 分類級聯處理 - 刪除中間層級應該只影響其子分類
    /// **Validates: Requirements 6.7**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategoryCascade_DeleteMiddleLevel_ShouldOnlyAffectDescendants(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        // Create two separate hierarchies
        var parent1Request = new CreateCategoryRequest(
            "Parent1", $"parent1-{random.Next(10000)}", null, null, null, null, 0);
        var parent1 = service.CreateCategoryAsync(parent1Request).Result;

        var child1Request = new CreateCategoryRequest(
            "Child1", $"child1-{random.Next(10000)}", parent1.Id, null, null, null, 0);
        var child1 = service.CreateCategoryAsync(child1Request).Result;

        var grandchild1Request = new CreateCategoryRequest(
            "Grandchild1", $"grandchild1-{random.Next(10000)}", child1.Id, null, null, null, 0);
        var grandchild1 = service.CreateCategoryAsync(grandchild1Request).Result;

        var parent2Request = new CreateCategoryRequest(
            "Parent2", $"parent2-{random.Next(10000)}", null, null, null, null, 0);
        var parent2 = service.CreateCategoryAsync(parent2Request).Result;

        // Act - Soft delete child1 (middle level of first hierarchy)
        service.SoftDeleteCategoryAsync(child1.Id).Wait();

        // Assert
        // Parent1 should still be visible
        var parent1Visible = service.GetCategoryByIdAsync(parent1.Id).Result != null;
        // Parent2 should still be visible
        var parent2Visible = service.GetCategoryByIdAsync(parent2.Id).Result != null;
        // Child1 and Grandchild1 should be deleted
        var child1Deleted = context.Categories
            .IgnoreQueryFilters()
            .First(c => c.Id == child1.Id).IsDeleted;
        var grandchild1Deleted = context.Categories
            .IgnoreQueryFilters()
            .First(c => c.Id == grandchild1.Id).IsDeleted;

        return parent1Visible && parent2Visible && child1Deleted && grandchild1Deleted;
    }

    #endregion
}
