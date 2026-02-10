using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Category;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 分類服務實作
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private const int MaxCategoryDepth = 3;
    private const int MaxCategoryNameLength = 20;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CategoryDto>> GetCategoriesAsync(QueryParameters query)
    {
        var queryable = _context.Categories.AsQueryable();

        // 是否包含已刪除的記錄
        if (query.IncludeDeleted)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                c.Slug.ToLower().Contains(searchTerm));
        }

        // 排序
        queryable = query.SortBy?.ToLower() switch
        {
            "name" => query.SortDescending
                ? queryable.OrderByDescending(c => c.Name)
                : queryable.OrderBy(c => c.Name),
            "level" => query.SortDescending
                ? queryable.OrderByDescending(c => c.Level)
                : queryable.OrderBy(c => c.Level),
            "sortorder" => query.SortDescending
                ? queryable.OrderByDescending(c => c.SortOrder)
                : queryable.OrderBy(c => c.SortOrder),
            "createdat" => query.SortDescending
                ? queryable.OrderByDescending(c => c.CreatedAt)
                : queryable.OrderBy(c => c.CreatedAt),
            _ => queryable.OrderBy(c => c.Level).ThenBy(c => c.SortOrder).ThenBy(c => c.Name)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => MapToDto(c))
            .ToListAsync();

        return new PagedResult<CategoryDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // 驗證分類名稱長度
        if (!ValidateCategoryName(request.Name))
        {
            throw new ArgumentException($"分類名稱最多 {MaxCategoryNameLength} 字元");
        }

        // 檢查是否可以新增子分類（最多 3 層）
        if (!await CanAddChildCategoryAsync(request.ParentId))
        {
            throw new InvalidOperationException($"分類層級已達上限（最多 {MaxCategoryDepth} 層）");
        }

        // 計算層級
        var level = 1;
        if (request.ParentId.HasValue)
        {
            var parentDepth = await GetCategoryDepthAsync(request.ParentId.Value);
            level = parentDepth + 1;
        }

        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            ParentId = request.ParentId,
            Level = level,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            SortOrder = request.SortOrder
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return null;

        // 驗證分類名稱長度
        if (!ValidateCategoryName(request.Name))
        {
            throw new ArgumentException($"分類名稱最多 {MaxCategoryNameLength} 字元");
        }

        // 如果父分類變更，需要重新計算層級並檢查限制
        if (category.ParentId != request.ParentId)
        {
            // 不能將分類設為自己的子分類
            if (request.ParentId == id)
            {
                throw new InvalidOperationException("分類不能設為自己的子分類");
            }

            // 檢查新的父分類是否會導致超過層級限制
            if (request.ParentId.HasValue)
            {
                var newParentDepth = await GetCategoryDepthAsync(request.ParentId.Value);
                var childrenMaxDepth = await GetMaxChildrenDepthAsync(id);
                
                if (newParentDepth + 1 + childrenMaxDepth > MaxCategoryDepth)
                {
                    throw new InvalidOperationException($"移動後分類層級將超過上限（最多 {MaxCategoryDepth} 層）");
                }

                category.Level = newParentDepth + 1;
            }
            else
            {
                category.Level = 1;
            }

            // 更新所有子分類的層級
            await UpdateChildrenLevelsAsync(id, category.Level);
        }

        category.Name = request.Name;
        category.Slug = request.Slug;
        category.ParentId = request.ParentId;
        category.MetaTitle = request.MetaTitle;
        category.MetaDescription = request.MetaDescription;
        category.MetaKeywords = request.MetaKeywords;
        category.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<bool> SoftDeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return false;

        // 級聯軟刪除所有子分類
        await CascadeSoftDeleteAsync(id);

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteCategoryAsync(int id)
    {
        var category = await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return false;

        // 級聯永久刪除所有子分類
        await CascadeHardDeleteAsync(id);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _context.Categories.Where(c => c.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        // 取得所有分類
        var allCategories = await _context.Categories
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // 建立樹狀結構
        return BuildTree(allCategories, null);
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Level)
            .ThenBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => MapToDto(c))
            .ToListAsync();
    }

    public async Task<int> GetCategoryDepthAsync(int categoryId)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        return category?.Level ?? 0;
    }

    public async Task<bool> CanAddChildCategoryAsync(int? parentId)
    {
        if (!parentId.HasValue)
        {
            // 根層級，可以新增
            return true;
        }

        var parentDepth = await GetCategoryDepthAsync(parentId.Value);
        // 如果父分類已經是第 3 層，則不能再新增子分類
        return parentDepth < MaxCategoryDepth;
    }

    public bool ValidateCategoryName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length <= MaxCategoryNameLength;
    }

    /// <summary>
    /// 取得子分類的最大深度（相對於當前分類）
    /// </summary>
    private async Task<int> GetMaxChildrenDepthAsync(int categoryId)
    {
        var children = await _context.Categories
            .Where(c => c.ParentId == categoryId)
            .ToListAsync();

        if (!children.Any())
            return 0;

        var maxDepth = 0;
        foreach (var child in children)
        {
            var childDepth = 1 + await GetMaxChildrenDepthAsync(child.Id);
            if (childDepth > maxDepth)
                maxDepth = childDepth;
        }

        return maxDepth;
    }

    /// <summary>
    /// 更新所有子分類的層級
    /// </summary>
    private async Task UpdateChildrenLevelsAsync(int parentId, int parentLevel)
    {
        var children = await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();

        foreach (var child in children)
        {
            child.Level = parentLevel + 1;
            await UpdateChildrenLevelsAsync(child.Id, child.Level);
        }
    }

    /// <summary>
    /// 級聯軟刪除所有子分類
    /// </summary>
    private async Task CascadeSoftDeleteAsync(int parentId)
    {
        var children = await _context.Categories
            .Where(c => c.ParentId == parentId)
            .ToListAsync();

        foreach (var child in children)
        {
            await CascadeSoftDeleteAsync(child.Id);
            child.IsDeleted = true;
            child.DeletedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// 級聯永久刪除所有子分類
    /// </summary>
    private async Task CascadeHardDeleteAsync(int parentId)
    {
        var children = await _context.Categories
            .IgnoreQueryFilters()
            .Where(c => c.ParentId == parentId)
            .ToListAsync();

        foreach (var child in children)
        {
            await CascadeHardDeleteAsync(child.Id);
            _context.Categories.Remove(child);
        }
    }

    private static List<CategoryTreeDto> BuildTree(List<Category> allCategories, int? parentId)
    {
        return allCategories
            .Where(c => c.ParentId == parentId)
            .Select(c => new CategoryTreeDto(
                c.Id,
                c.Name,
                c.Slug,
                c.ParentId,
                c.Level,
                c.MetaTitle,
                c.MetaDescription,
                c.MetaKeywords,
                c.SortOrder,
                c.CreatedAt,
                c.UpdatedAt,
                BuildTree(allCategories, c.Id)))
            .ToList();
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentId,
            category.Level,
            category.MetaTitle,
            category.MetaDescription,
            category.MetaKeywords,
            category.SortOrder,
            category.CreatedAt,
            category.UpdatedAt);
    }
}
