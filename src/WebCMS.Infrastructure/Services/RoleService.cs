using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Role;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 角色服務實作
/// </summary>
public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RoleDto>> GetRolesAsync(QueryParameters query)
    {
        var queryable = _context.Roles.AsQueryable();

        // 是否包含已刪除的記錄
        if (query.IncludeDeleted)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(r =>
                r.Name.ToLower().Contains(searchTerm) ||
                (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
        }

        // 排序
        queryable = query.SortBy?.ToLower() switch
        {
            "name" => query.SortDescending
                ? queryable.OrderByDescending(r => r.Name)
                : queryable.OrderBy(r => r.Name),
            "hierarchylevel" => query.SortDescending
                ? queryable.OrderByDescending(r => r.HierarchyLevel)
                : queryable.OrderBy(r => r.HierarchyLevel),
            "createdat" => query.SortDescending
                ? queryable.OrderByDescending(r => r.CreatedAt)
                : queryable.OrderBy(r => r.CreatedAt),
            _ => queryable.OrderBy(r => r.HierarchyLevel)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => MapToDto(r))
            .ToListAsync();

        return new PagedResult<RoleDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            HierarchyLevel = request.HierarchyLevel
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return null;

        role.Name = request.Name;
        role.Description = request.Description;
        role.HierarchyLevel = request.HierarchyLevel;

        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<bool> SoftDeleteRoleAsync(int id)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role == null)
            return false;

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
            return false;

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.Roles.Where(r => r.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        return await _context.Roles
            .OrderBy(r => r.HierarchyLevel)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    public async Task<List<RoleDto>> GetRolesByHierarchyLevelAsync(int maxLevel)
    {
        return await _context.Roles
            .Where(r => r.HierarchyLevel <= maxLevel)
            .OrderBy(r => r.HierarchyLevel)
            .Select(r => MapToDto(r))
            .ToListAsync();
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto(
            role.Id,
            role.Name,
            role.Description,
            role.HierarchyLevel,
            role.CreatedAt,
            role.UpdatedAt);
    }
}
