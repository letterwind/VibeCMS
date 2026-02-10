using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.Entities;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Role;

/// <summary>
/// 角色管理模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class RolePropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    #region Property 8: 角色階層等級

    /// <summary>
    /// Property 8: 角色階層等級
    /// 對於任何角色集合，系統應該正確維護階層等級的順序關係，較低等級的角色不應擁有較高等級角色的權限。
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool RoleHierarchy_ShouldMaintainCorrectOrder(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        // Create roles with different hierarchy levels
        var roles = new List<(string Name, int Level)>
        {
            ("Admin", 1),
            ("Manager", 2),
            ("Finance", 3),
            ("User", 4)
        };

        // Shuffle and create roles in random order
        var shuffledRoles = roles.OrderBy(_ => random.Next()).ToList();
        foreach (var (name, level) in shuffledRoles)
        {
            var request = new CreateRoleRequest(name, $"{name} role", level);
            service.CreateRoleAsync(request).Wait();
        }

        // Act - Get all roles ordered by hierarchy level
        var result = service.GetAllRolesAsync().Result;

        // Assert - Roles should be ordered by hierarchy level (ascending)
        var orderedLevels = result.Select(r => r.HierarchyLevel).ToList();
        var expectedOrder = orderedLevels.OrderBy(l => l).ToList();

        return orderedLevels.SequenceEqual(expectedOrder);
    }

    /// <summary>
    /// Property 8: 角色階層等級 - 階層等級篩選
    /// 系統應該能夠根據最大階層等級篩選角色。
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool RoleHierarchy_FilterByMaxLevel_ShouldReturnCorrectRoles(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        // Create roles with hierarchy levels 1-5
        for (int i = 1; i <= 5; i++)
        {
            var request = new CreateRoleRequest($"Role{i}", $"Role at level {i}", i);
            service.CreateRoleAsync(request).Wait();
        }

        // Pick a random max level between 1 and 5
        var maxLevel = random.Next(1, 6);

        // Act
        var result = service.GetRolesByHierarchyLevelAsync(maxLevel).Result;

        // Assert - All returned roles should have hierarchy level <= maxLevel
        return result.All(r => r.HierarchyLevel <= maxLevel) &&
               result.Count == maxLevel; // Should have exactly maxLevel roles
    }

    /// <summary>
    /// Property 8: 角色階層等級 - 階層等級必須為正整數
    /// **Validates: Requirements 2.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool RoleHierarchy_LevelMustBePositive(PositiveInt level)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);

        var request = new CreateRoleRequest("TestRole", "Test description", level.Get);

        // Act
        var result = service.CreateRoleAsync(request).Result;

        // Assert - Role should be created with the specified positive level
        return result.HierarchyLevel == level.Get && result.HierarchyLevel > 0;
    }

    #endregion

    #region Property 9: 軟刪除機制

    /// <summary>
    /// Property 9: 軟刪除機制
    /// 對於任何被刪除的角色，系統應該保留該資料並標記為已刪除，而非實際從資料庫移除。
    /// **Validates: Requirements 2.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SoftDelete_ShouldMarkAsDeletedNotRemove(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        var roleName = $"TestRole{random.Next(10000)}";
        var request = new CreateRoleRequest(roleName, "Test description", random.Next(1, 10));
        var createdRole = service.CreateRoleAsync(request).Result;

        // Act - Soft delete the role
        var deleteResult = service.SoftDeleteRoleAsync(createdRole.Id).Result;

        // Assert
        // 1. Delete operation should succeed
        if (!deleteResult) return false;

        // 2. Role should not be visible in normal queries
        var normalQuery = service.GetRoleByIdAsync(createdRole.Id).Result;
        if (normalQuery != null) return false;

        // 3. Role should still exist in database when including deleted
        var deletedRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Id == createdRole.Id);

        return deletedRole != null &&
               deletedRole.IsDeleted &&
               deletedRole.DeletedAt != null;
    }

    /// <summary>
    /// Property 9: 軟刪除機制 - 軟刪除的角色可以透過 IncludeDeleted 查詢
    /// **Validates: Requirements 2.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SoftDelete_ShouldBeVisibleWithIncludeDeleted(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        // Create multiple roles
        var roleCount = random.Next(3, 8);
        var createdRoleIds = new List<int>();
        for (int i = 0; i < roleCount; i++)
        {
            var request = new CreateRoleRequest($"Role{i}_{random.Next(10000)}", $"Description {i}", i + 1);
            var role = service.CreateRoleAsync(request).Result;
            createdRoleIds.Add(role.Id);
        }

        // Soft delete some roles
        var deleteCount = random.Next(1, roleCount);
        var deletedIds = createdRoleIds.Take(deleteCount).ToList();
        foreach (var id in deletedIds)
        {
            service.SoftDeleteRoleAsync(id).Wait();
        }

        // Act - Query with IncludeDeleted = true
        var queryWithDeleted = new QueryParameters(IncludeDeleted: true);
        var resultWithDeleted = service.GetRolesAsync(queryWithDeleted).Result;

        // Query without IncludeDeleted
        var queryWithoutDeleted = new QueryParameters(IncludeDeleted: false);
        var resultWithoutDeleted = service.GetRolesAsync(queryWithoutDeleted).Result;

        // Assert
        return resultWithDeleted.TotalCount == roleCount &&
               resultWithoutDeleted.TotalCount == roleCount - deleteCount;
    }

    /// <summary>
    /// Property 9: 軟刪除機制 - 永久刪除應該從資料庫移除記錄
    /// **Validates: Requirements 2.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool HardDelete_ShouldRemoveFromDatabase(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        var roleName = $"TestRole{random.Next(10000)}";
        var request = new CreateRoleRequest(roleName, "Test description", random.Next(1, 10));
        var createdRole = service.CreateRoleAsync(request).Result;

        // First soft delete
        service.SoftDeleteRoleAsync(createdRole.Id).Wait();

        // Act - Hard delete the role
        var hardDeleteResult = service.HardDeleteRoleAsync(createdRole.Id).Result;

        // Assert
        // 1. Hard delete operation should succeed
        if (!hardDeleteResult) return false;

        // 2. Role should not exist even when ignoring query filters
        var deletedRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Id == createdRole.Id);

        return deletedRole == null;
    }

    /// <summary>
    /// Property 9: 軟刪除機制 - 軟刪除應該設定 DeletedAt 時間戳
    /// **Validates: Requirements 2.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SoftDelete_ShouldSetDeletedAtTimestamp(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new RoleService(context);
        var random = new Random(seed.Get);

        var request = new CreateRoleRequest($"TestRole{random.Next(10000)}", "Test", random.Next(1, 10));
        var createdRole = service.CreateRoleAsync(request).Result;

        var beforeDelete = DateTime.UtcNow;

        // Act
        service.SoftDeleteRoleAsync(createdRole.Id).Wait();

        var afterDelete = DateTime.UtcNow;

        // Assert
        var deletedRole = context.Roles
            .IgnoreQueryFilters()
            .First(r => r.Id == createdRole.Id);

        return deletedRole.DeletedAt != null &&
               deletedRole.DeletedAt >= beforeDelete &&
               deletedRole.DeletedAt <= afterDelete;
    }

    #endregion
}
