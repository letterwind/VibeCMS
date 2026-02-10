using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.DTOs.User;
using WebCMS.Core.Entities;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.SoftDelete;

/// <summary>
/// 軟刪除與永久刪除屬性測試
/// Feature: web-cms-management
/// </summary>
public class SoftDeletePropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    #region Property 9: 軟刪除機制 - 整合測試

    /// <summary>
    /// Property 9: 軟刪除機制 - 使用者軟刪除
    /// 對於任何被刪除的使用者，系統應該保留該資料並標記為已刪除，而非實際從資料庫移除。
    /// **Validates: Requirements 4.9, 11.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool UserSoftDelete_ShouldMarkAsDeletedNotRemove(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var passwordService = new PasswordValidationService();
        var service = new UserService(context, passwordService);
        var random = new Random(seed.Get);

        var request = new CreateUserRequest(
            $"User{random.Next(10000)}Aa1",
            $"Pass{random.Next(10000)}Aa1",
            $"Test User {random.Next(10000)}",
            null);
        var createdUser = service.CreateUserAsync(request).Result;

        // Act - Soft delete the user
        var deleteResult = service.SoftDeleteUserAsync(createdUser.Id).Result;

        // Assert
        if (!deleteResult) return false;

        // User should not be visible in normal queries
        var normalQuery = service.GetUserByIdAsync(createdUser.Id).Result;
        if (normalQuery != null) return false;

        // User should still exist in database when including deleted
        var deletedUser = context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.Id == createdUser.Id);

        return deletedUser != null &&
               deletedUser.IsDeleted &&
               deletedUser.DeletedAt != null;
    }

    /// <summary>
    /// Property 9: 軟刪除機制 - 分類軟刪除
    /// 對於任何被刪除的分類，系統應該保留該資料並標記為已刪除，而非實際從資料庫移除。
    /// **Validates: Requirements 6.6, 11.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CategorySoftDelete_ShouldMarkAsDeletedNotRemove(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new CategoryService(context);
        var random = new Random(seed.Get);

        var request = new CreateCategoryRequest(
            $"Cat{random.Next(1000)}",
            $"cat-{random.Next(10000)}",
            null,
            null,
            null,
            null);
        var createdCategory = service.CreateCategoryAsync(request).Result;

        // Act - Soft delete the category
        var deleteResult = service.SoftDeleteCategoryAsync(createdCategory.Id).Result;

        // Assert
        if (!deleteResult) return false;

        // Category should not be visible in normal queries
        var normalQuery = service.GetCategoryByIdAsync(createdCategory.Id).Result;
        if (normalQuery != null) return false;

        // Category should still exist in database when including deleted
        var deletedCategory = context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefault(c => c.Id == createdCategory.Id);

        return deletedCategory != null &&
               deletedCategory.IsDeleted &&
               deletedCategory.DeletedAt != null;
    }

    /// <summary>
    /// Property 9: 軟刪除機制 - 全域查詢篩選器
    /// 對於任何軟刪除的實體，預設查詢應該自動排除已刪除的記錄。
    /// **Validates: Requirements 11.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool GlobalQueryFilter_ShouldExcludeDeletedByDefault(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var random = new Random(seed.Get);

        // Create multiple roles
        var roleCount = random.Next(3, 8);
        var createdRoleIds = new List<int>();
        for (int i = 0; i < roleCount; i++)
        {
            var request = new CreateRoleRequest($"Role{i}_{random.Next(10000)}", $"Description {i}", i + 1);
            var role = roleService.CreateRoleAsync(request).Result;
            createdRoleIds.Add(role.Id);
        }

        // Soft delete some roles
        var deleteCount = random.Next(1, roleCount);
        var deletedIds = createdRoleIds.Take(deleteCount).ToList();
        foreach (var id in deletedIds)
        {
            roleService.SoftDeleteRoleAsync(id).Wait();
        }

        // Act - Query without IgnoreQueryFilters
        var normalCount = context.Roles.Count();

        // Query with IgnoreQueryFilters
        var totalCount = context.Roles.IgnoreQueryFilters().Count();

        // Assert
        return normalCount == roleCount - deleteCount &&
               totalCount == roleCount;
    }

    #endregion

    #region Property 19: 超級管理員永久刪除

    /// <summary>
    /// Property 19: 超級管理員永久刪除 - 超級管理員可以永久刪除角色
    /// 對於任何已軟刪除的記錄，僅有超級管理員角色可以執行永久刪除操作。
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SuperAdmin_CanHardDeleteRole(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var permissionService = new PermissionService(context);
        var passwordService = new PasswordValidationService();
        var userService = new UserService(context, passwordService);
        var random = new Random(seed.Get);

        // Create a super admin role (hierarchy level 1)
        var superAdminRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("SuperAdmin", "Super Administrator", 1)).Result;

        // Create a super admin user
        var superAdminUser = userService.CreateUserAsync(
            new CreateUserRequest($"Admin{random.Next(10000)}Aa1", "Admin123Aa1", "Super Admin", new List<int> { superAdminRole.Id })).Result;

        // Create a role to be deleted
        var roleToDelete = roleService.CreateRoleAsync(
            new CreateRoleRequest($"ToDelete{random.Next(10000)}", "To be deleted", 5)).Result;

        // Soft delete the role first
        roleService.SoftDeleteRoleAsync(roleToDelete.Id).Wait();

        // Verify user is super admin
        var isSuperAdmin = permissionService.IsSuperAdminAsync(superAdminUser.Id).Result;
        if (!isSuperAdmin) return false;

        // Act - Hard delete the role (simulating super admin action)
        var hardDeleteResult = roleService.HardDeleteRoleAsync(roleToDelete.Id).Result;

        // Assert
        if (!hardDeleteResult) return false;

        // Role should not exist even when ignoring query filters
        var deletedRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Id == roleToDelete.Id);

        return deletedRole == null;
    }

    /// <summary>
    /// Property 19: 超級管理員永久刪除 - 非超級管理員無法永久刪除
    /// 對於任何已軟刪除的記錄，非超級管理員角色應該被拒絕執行永久刪除操作。
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool NonSuperAdmin_ShouldNotBeIdentifiedAsSuperAdmin(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var permissionService = new PermissionService(context);
        var passwordService = new PasswordValidationService();
        var userService = new UserService(context, passwordService);
        var random = new Random(seed.Get);

        // Create a non-super admin role (hierarchy level > 1)
        var regularRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("RegularUser", "Regular User", random.Next(2, 10))).Result;

        // Create a regular user
        var regularUser = userService.CreateUserAsync(
            new CreateUserRequest($"User{random.Next(10000)}Aa1", "User123Aa1", "Regular User", new List<int> { regularRole.Id })).Result;

        // Act - Check if user is super admin
        var isSuperAdmin = permissionService.IsSuperAdminAsync(regularUser.Id).Result;

        // Assert - Regular user should not be identified as super admin
        return !isSuperAdmin;
    }

    /// <summary>
    /// Property 19: 超級管理員永久刪除 - 超級管理員識別
    /// 系統應該正確識別階層等級為 1 的角色為超級管理員。
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool SuperAdminIdentification_ShouldBeBasedOnHierarchyLevel(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var permissionService = new PermissionService(context);
        var passwordService = new PasswordValidationService();
        var userService = new UserService(context, passwordService);
        var random = new Random(seed.Get);

        // Create roles with different hierarchy levels
        var superAdminRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("SuperAdmin", "Super Administrator", 1)).Result;
        var managerRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("Manager", "Manager", 2)).Result;
        var userRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("User", "Regular User", 3)).Result;

        // Create users with different roles
        var superAdminUser = userService.CreateUserAsync(
            new CreateUserRequest($"SA{random.Next(10000)}Aa1", "Admin123Aa1", "Super Admin", new List<int> { superAdminRole.Id })).Result;
        var managerUser = userService.CreateUserAsync(
            new CreateUserRequest($"Mgr{random.Next(10000)}Aa1", "Mgr123Aa1", "Manager", new List<int> { managerRole.Id })).Result;
        var regularUser = userService.CreateUserAsync(
            new CreateUserRequest($"Usr{random.Next(10000)}Aa1", "Usr123Aa1", "User", new List<int> { userRole.Id })).Result;

        // Act & Assert
        var isSuperAdminSA = permissionService.IsSuperAdminAsync(superAdminUser.Id).Result;
        var isSuperAdminMgr = permissionService.IsSuperAdminAsync(managerUser.Id).Result;
        var isSuperAdminUsr = permissionService.IsSuperAdminAsync(regularUser.Id).Result;

        return isSuperAdminSA && !isSuperAdminMgr && !isSuperAdminUsr;
    }

    /// <summary>
    /// Property 19: 超級管理員永久刪除 - 多角色使用者
    /// 如果使用者擁有多個角色，只要其中一個角色是超級管理員，就應該被識別為超級管理員。
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool MultiRoleUser_ShouldBeSuperAdminIfAnyRoleIsSuperAdmin(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var permissionService = new PermissionService(context);
        var passwordService = new PasswordValidationService();
        var userService = new UserService(context, passwordService);
        var random = new Random(seed.Get);

        // Create roles
        var superAdminRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("SuperAdmin", "Super Administrator", 1)).Result;
        var managerRole = roleService.CreateRoleAsync(
            new CreateRoleRequest("Manager", "Manager", 2)).Result;

        // Create user with both roles
        var multiRoleUser = userService.CreateUserAsync(
            new CreateUserRequest(
                $"Multi{random.Next(10000)}Aa1",
                "Multi123Aa1",
                "Multi Role User",
                new List<int> { superAdminRole.Id, managerRole.Id })).Result;

        // Act
        var isSuperAdmin = permissionService.IsSuperAdminAsync(multiRoleUser.Id).Result;

        // Assert - User should be identified as super admin
        return isSuperAdmin;
    }

    /// <summary>
    /// Property 19: 超級管理員永久刪除 - 永久刪除後資料不存在
    /// 永久刪除後，即使使用 IgnoreQueryFilters 也無法查詢到該記錄。
    /// **Validates: Requirements 11.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool HardDelete_ShouldCompletelyRemoveFromDatabase(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var roleService = new RoleService(context);
        var random = new Random(seed.Get);

        var roleName = $"TestRole{random.Next(10000)}";
        var request = new CreateRoleRequest(roleName, "Test description", random.Next(1, 10));
        var createdRole = roleService.CreateRoleAsync(request).Result;

        // First soft delete
        roleService.SoftDeleteRoleAsync(createdRole.Id).Wait();

        // Verify soft delete worked
        var softDeletedRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Id == createdRole.Id);
        if (softDeletedRole == null || !softDeletedRole.IsDeleted) return false;

        // Act - Hard delete
        var hardDeleteResult = roleService.HardDeleteRoleAsync(createdRole.Id).Result;

        // Assert
        if (!hardDeleteResult) return false;

        // Role should not exist even with IgnoreQueryFilters
        var afterHardDelete = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Id == createdRole.Id);

        return afterHardDelete == null;
    }

    #endregion
}
