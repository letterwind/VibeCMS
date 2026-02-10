using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Permission;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;
using WebCMS.Infrastructure.Services;

namespace WebCMS.Tests.Permission;

/// <summary>
/// 權限管理模組屬性測試
/// Feature: web-cms-management
/// </summary>
public class PermissionPropertyTests
{
    private ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private async Task<(Core.Entities.Role role, Core.Entities.Function function)> SetupRoleAndFunction(ApplicationDbContext context, int seed)
    {
        var random = new Random(seed);
        
        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            Description = "Test role",
            HierarchyLevel = random.Next(1, 10)
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = $"TestFunction_{random.Next(10000)}",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test",
            SortOrder = 1
        };
        context.Functions.Add(function);

        await context.SaveChangesAsync();
        return (role, function);
    }

    #region Property 10: CRUD 權限設定

    /// <summary>
    /// Property 10: CRUD 權限設定
    /// 對於任何功能項目，系統應該提供獨立的新增、讀取、更新、刪除四種權限設定，且每種權限可以獨立啟用或停用。
    /// **Validates: Requirements 3.1, 3.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CrudPermissions_ShouldBeIndependentlyConfigurable(
        PositiveInt seed,
        bool canCreate,
        bool canRead,
        bool canUpdate,
        bool canDelete)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var (role, function) = SetupRoleAndFunction(context, seed.Get).Result;

        var permissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, canCreate, canRead, canUpdate, canDelete)
        };

        // Act
        service.SetPermissionsAsync(role.Id, permissions).Wait();
        var result = service.GetPermissionsByRoleAsync(role.Id).Result;

        // Assert - If all permissions are false, no record should be created
        if (!canCreate && !canRead && !canUpdate && !canDelete)
        {
            return result.Count == 0;
        }

        // Otherwise, verify each permission is set correctly
        var permission = result.FirstOrDefault(p => p.FunctionId == function.Id);
        return permission != null &&
               permission.CanCreate == canCreate &&
               permission.CanRead == canRead &&
               permission.CanUpdate == canUpdate &&
               permission.CanDelete == canDelete;
    }

    /// <summary>
    /// Property 10: CRUD 權限設定 - 多功能權限設定
    /// 系統應該能夠同時設定多個功能的權限。
    /// **Validates: Requirements 3.1, 3.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CrudPermissions_ShouldSupportMultipleFunctions(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            Description = "Test role",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        // Create multiple functions
        var functionCount = random.Next(2, 6);
        var functions = new List<Core.Entities.Function>();
        for (int i = 0; i < functionCount; i++)
        {
            var function = new Core.Entities.Function
            {
                Name = $"Function_{i}_{random.Next(10000)}",
                Code = $"FUNC_{i}_{random.Next(10000)}",
                Url = $"/test/{i}",
                SortOrder = i
            };
            context.Functions.Add(function);
            functions.Add(function);
        }
        context.SaveChanges();

        // Set random permissions for each function
        var permissionSettings = functions.Select(f => new PermissionSetting(
            f.Id,
            random.Next(2) == 1,
            random.Next(2) == 1,
            random.Next(2) == 1,
            random.Next(2) == 1
        )).ToList();

        // Act
        service.SetPermissionsAsync(role.Id, permissionSettings).Wait();
        var result = service.GetPermissionsByRoleAsync(role.Id).Result;

        // Assert - Verify each permission setting
        foreach (var setting in permissionSettings)
        {
            // Skip if all permissions are false (no record created)
            if (!setting.CanCreate && !setting.CanRead && !setting.CanUpdate && !setting.CanDelete)
                continue;

            var permission = result.FirstOrDefault(p => p.FunctionId == setting.FunctionId);
            if (permission == null ||
                permission.CanCreate != setting.CanCreate ||
                permission.CanRead != setting.CanRead ||
                permission.CanUpdate != setting.CanUpdate ||
                permission.CanDelete != setting.CanDelete)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Property 10: CRUD 權限設定 - 權限更新應該覆蓋舊設定
    /// **Validates: Requirements 3.1, 3.2**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool CrudPermissions_UpdateShouldOverwriteOldSettings(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var (role, function) = SetupRoleAndFunction(context, seed.Get).Result;

        // Set initial permissions
        var initialPermissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, true, true, true, true)
        };
        service.SetPermissionsAsync(role.Id, initialPermissions).Wait();

        // Act - Update with new permissions
        var newPermissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, false, true, false, true)
        };
        service.SetPermissionsAsync(role.Id, newPermissions).Wait();
        var result = service.GetPermissionsByRoleAsync(role.Id).Result;

        // Assert
        var permission = result.FirstOrDefault(p => p.FunctionId == function.Id);
        return permission != null &&
               permission.CanCreate == false &&
               permission.CanRead == true &&
               permission.CanUpdate == false &&
               permission.CanDelete == true;
    }

    #endregion

    #region Property 11: 權限存取控制

    /// <summary>
    /// Property 11: 權限存取控制
    /// 對於任何使用者嘗試存取的功能，若該使用者的角色沒有對應的權限，系統應該拒絕存取。
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionAccessControl_ShouldDenyWithoutPermission(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create user, role, and function
        var user = new Core.Entities.AdminUser
        {
            Account = $"user_{random.Next(10000)}",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };
        context.AdminUsers.Add(user);

        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Assign role to user
        context.UserRoles.Add(new Core.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
        context.SaveChanges();

        // Do NOT set any permissions

        // Act & Assert - All permission types should be denied
        var hasCreate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Create).Result;
        var hasRead = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        var hasUpdate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Update).Result;
        var hasDelete = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Delete).Result;

        return !hasCreate && !hasRead && !hasUpdate && !hasDelete;
    }

    /// <summary>
    /// Property 11: 權限存取控制 - 有權限時應該允許存取
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionAccessControl_ShouldAllowWithPermission(
        PositiveInt seed,
        bool canCreate,
        bool canRead,
        bool canUpdate,
        bool canDelete)
    {
        // Skip if all permissions are false
        if (!canCreate && !canRead && !canUpdate && !canDelete)
            return true;

        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create user, role, and function
        var user = new Core.Entities.AdminUser
        {
            Account = $"user_{random.Next(10000)}",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };
        context.AdminUsers.Add(user);

        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Assign role to user
        context.UserRoles.Add(new Core.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
        context.SaveChanges();

        // Set permissions
        var permissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, canCreate, canRead, canUpdate, canDelete)
        };
        service.SetPermissionsAsync(role.Id, permissions).Wait();

        // Act & Assert
        var hasCreate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Create).Result;
        var hasRead = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        var hasUpdate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Update).Result;
        var hasDelete = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Delete).Result;

        return hasCreate == canCreate &&
               hasRead == canRead &&
               hasUpdate == canUpdate &&
               hasDelete == canDelete;
    }

    /// <summary>
    /// Property 11: 權限存取控制 - 無角色的使用者應該被拒絕
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionAccessControl_UserWithoutRoleShouldBeDenied(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create user without role
        var user = new Core.Entities.AdminUser
        {
            Account = $"user_{random.Next(10000)}",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };
        context.AdminUsers.Add(user);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Act & Assert - All permission types should be denied
        var hasCreate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Create).Result;
        var hasRead = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        var hasUpdate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Update).Result;
        var hasDelete = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Delete).Result;

        return !hasCreate && !hasRead && !hasUpdate && !hasDelete;
    }

    #endregion

    #region Property 12: 權限即時生效

    /// <summary>
    /// Property 12: 權限即時生效
    /// 對於任何權限設定的變更，該變更應該立即影響所有擁有該角色的使用者，無需重新登入。
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionImmediateEffect_ChangesShouldApplyImmediately(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create user, role, and function
        var user = new Core.Entities.AdminUser
        {
            Account = $"user_{random.Next(10000)}",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };
        context.AdminUsers.Add(user);

        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Assign role to user
        context.UserRoles.Add(new Core.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
        context.SaveChanges();

        // Initially no permissions
        var hasReadBefore = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        if (hasReadBefore) return false; // Should not have permission initially

        // Act - Grant read permission
        var permissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, false, true, false, false)
        };
        service.SetPermissionsAsync(role.Id, permissions).Wait();

        // Assert - Permission should be immediately effective
        var hasReadAfter = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        return hasReadAfter;
    }

    /// <summary>
    /// Property 12: 權限即時生效 - 權限撤銷應該立即生效
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionImmediateEffect_RevocationShouldApplyImmediately(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create user, role, and function
        var user = new Core.Entities.AdminUser
        {
            Account = $"user_{random.Next(10000)}",
            PasswordHash = "hash",
            DisplayName = "Test User"
        };
        context.AdminUsers.Add(user);

        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Assign role to user
        context.UserRoles.Add(new Core.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
        context.SaveChanges();

        // Grant all permissions initially
        var initialPermissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, true, true, true, true)
        };
        service.SetPermissionsAsync(role.Id, initialPermissions).Wait();

        var hasAllBefore = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Create).Result &&
                          service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result &&
                          service.HasPermissionAsync(user.Id, function.Code, PermissionType.Update).Result &&
                          service.HasPermissionAsync(user.Id, function.Code, PermissionType.Delete).Result;
        if (!hasAllBefore) return false; // Should have all permissions initially

        // Act - Revoke all permissions
        var revokedPermissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, false, false, false, false)
        };
        service.SetPermissionsAsync(role.Id, revokedPermissions).Wait();

        // Assert - All permissions should be immediately revoked
        var hasCreate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Create).Result;
        var hasRead = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
        var hasUpdate = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Update).Result;
        var hasDelete = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Delete).Result;

        return !hasCreate && !hasRead && !hasUpdate && !hasDelete;
    }

    /// <summary>
    /// Property 12: 權限即時生效 - 多使用者應該同時受影響
    /// **Validates: Requirements 3.4**
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PermissionImmediateEffect_MultipleUsersShouldBeAffected(PositiveInt seed)
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new PermissionService(context);
        var random = new Random(seed.Get);

        // Create role and function
        var role = new Core.Entities.Role
        {
            Name = $"TestRole_{random.Next(10000)}",
            HierarchyLevel = 1
        };
        context.Roles.Add(role);

        var function = new Core.Entities.Function
        {
            Name = "TestFunction",
            Code = $"FUNC_{random.Next(10000)}",
            Url = "/test"
        };
        context.Functions.Add(function);
        context.SaveChanges();

        // Create multiple users with the same role
        var userCount = random.Next(2, 5);
        var users = new List<Core.Entities.AdminUser>();
        for (int i = 0; i < userCount; i++)
        {
            var user = new Core.Entities.AdminUser
            {
                Account = $"user_{i}_{random.Next(10000)}",
                PasswordHash = "hash",
                DisplayName = $"Test User {i}"
            };
            context.AdminUsers.Add(user);
            users.Add(user);
        }
        context.SaveChanges();

        // Assign role to all users
        foreach (var user in users)
        {
            context.UserRoles.Add(new Core.Entities.UserRole { UserId = user.Id, RoleId = role.Id });
        }
        context.SaveChanges();

        // Act - Grant read permission
        var permissions = new List<PermissionSetting>
        {
            new PermissionSetting(function.Id, false, true, false, false)
        };
        service.SetPermissionsAsync(role.Id, permissions).Wait();

        // Assert - All users should have the permission
        foreach (var user in users)
        {
            var hasRead = service.HasPermissionAsync(user.Id, function.Code, PermissionType.Read).Result;
            if (!hasRead) return false;
        }

        return true;
    }

    #endregion
}
