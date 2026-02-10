using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.DTOs.User;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 使用者服務實作
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordValidationService _passwordValidationService;

    public UserService(
        ApplicationDbContext context,
        IPasswordValidationService passwordValidationService)
    {
        _context = context;
        _passwordValidationService = passwordValidationService;
    }

    public async Task<PagedResult<UserDto>> GetUsersAsync(QueryParameters query)
    {
        var queryable = _context.AdminUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        // 是否包含已刪除的記錄
        if (query.IncludeDeleted)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(u =>
                u.Account.ToLower().Contains(searchTerm) ||
                u.DisplayName.ToLower().Contains(searchTerm));
        }

        // 排序
        queryable = query.SortBy?.ToLower() switch
        {
            "account" => query.SortDescending
                ? queryable.OrderByDescending(u => u.Account)
                : queryable.OrderBy(u => u.Account),
            "displayname" => query.SortDescending
                ? queryable.OrderByDescending(u => u.DisplayName)
                : queryable.OrderBy(u => u.DisplayName),
            "createdat" => query.SortDescending
                ? queryable.OrderByDescending(u => u.CreatedAt)
                : queryable.OrderBy(u => u.CreatedAt),
            _ => queryable.OrderBy(u => u.Account)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResult<UserDto>(
            dtos,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.AdminUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        var user = new AdminUser
        {
            Account = request.Account,
            PasswordHash = HashPassword(request.Password),
            DisplayName = request.DisplayName,
            PasswordLastChanged = DateTime.UtcNow
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // 設定角色
        if (request.RoleIds != null && request.RoleIds.Count > 0)
        {
            foreach (var roleId in request.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
            await _context.SaveChangesAsync();
        }

        // 重新載入使用者以取得角色資訊
        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.AdminUsers
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        user.DisplayName = request.DisplayName;

        // 更新密碼（如果提供）
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            user.PasswordHash = HashPassword(request.NewPassword);
            user.PasswordLastChanged = DateTime.UtcNow;
        }

        // 更新角色
        if (request.RoleIds != null)
        {
            // 移除現有角色
            _context.UserRoles.RemoveRange(user.UserRoles);

            // 新增新角色
            foreach (var roleId in request.RoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                });
            }
        }

        await _context.SaveChangesAsync();

        // 重新載入使用者以取得角色資訊
        return await GetUserByIdAsync(user.Id);
    }

    public async Task<bool> SoftDeleteUserAsync(int id)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return false;

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteUserAsync(int id)
    {
        var user = await _context.AdminUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return false;

        _context.AdminUsers.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsAccountExistsAsync(string account, int? excludeId = null)
    {
        var query = _context.AdminUsers.Where(u => u.Account == account);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> IsPasswordExpiredAsync(int userId)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        return _passwordValidationService.IsPasswordExpired(user.PasswordLastChanged);
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        user.PasswordHash = HashPassword(newPassword);
        user.PasswordLastChanged = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public ValidationResult ValidateAccount(string account)
    {
        return _passwordValidationService.ValidateAccount(account);
    }

    public ValidationResult ValidatePassword(string password, string account)
    {
        return _passwordValidationService.ValidatePassword(password, account);
    }

    public async Task<bool> IsSameAsCurrentPasswordAsync(int userId, string newPassword)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        return _passwordValidationService.IsSameAsCurrentPassword(newPassword, user.PasswordHash);
    }

    private UserDto MapToDto(AdminUser user)
    {
        var roles = user.UserRoles
            .Where(ur => ur.Role != null && !ur.Role.IsDeleted)
            .Select(ur => new RoleDto(
                ur.Role.Id,
                ur.Role.Name,
                ur.Role.Description,
                ur.Role.HierarchyLevel,
                ur.Role.CreatedAt,
                ur.Role.UpdatedAt))
            .ToList();

        var isPasswordExpired = _passwordValidationService.IsPasswordExpired(user.PasswordLastChanged);

        return new UserDto(
            user.Id,
            user.Account,
            user.DisplayName,
            roles,
            isPasswordExpired,
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }
}
