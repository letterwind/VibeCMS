using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.Function;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 功能服務實作
/// </summary>
public class FunctionService : IFunctionService
{
    private readonly ApplicationDbContext _context;

    public FunctionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FunctionDto>> GetFunctionsAsync(QueryParameters query)
    {
        var queryable = _context.Functions.AsQueryable();

        // 是否包含已刪除的記錄
        if (query.IncludeDeleted)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(f =>
                f.Name.ToLower().Contains(searchTerm) ||
                f.Code.ToLower().Contains(searchTerm));
        }

        // 排序
        queryable = query.SortBy?.ToLower() switch
        {
            "name" => query.SortDescending
                ? queryable.OrderByDescending(f => f.Name)
                : queryable.OrderBy(f => f.Name),
            "code" => query.SortDescending
                ? queryable.OrderByDescending(f => f.Code)
                : queryable.OrderBy(f => f.Code),
            "sortorder" => query.SortDescending
                ? queryable.OrderByDescending(f => f.SortOrder)
                : queryable.OrderBy(f => f.SortOrder),
            "createdat" => query.SortDescending
                ? queryable.OrderByDescending(f => f.CreatedAt)
                : queryable.OrderBy(f => f.CreatedAt),
            _ => queryable.OrderBy(f => f.SortOrder).ThenBy(f => f.Name)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(f => MapToDto(f))
            .ToListAsync();

        return new PagedResult<FunctionDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }


    public async Task<FunctionDto?> GetFunctionByIdAsync(int id)
    {
        var function = await _context.Functions.FirstOrDefaultAsync(f => f.Id == id);
        return function == null ? null : MapToDto(function);
    }

    public async Task<FunctionDto> CreateFunctionAsync(CreateFunctionRequest request)
    {
        var function = new Function
        {
            Name = request.Name,
            Code = request.Code,
            Url = request.Url,
            OpenInNewWindow = request.OpenInNewWindow,
            Icon = request.Icon,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        _context.Functions.Add(function);
        await _context.SaveChangesAsync();

        return MapToDto(function);
    }

    public async Task<FunctionDto?> UpdateFunctionAsync(int id, UpdateFunctionRequest request)
    {
        var function = await _context.Functions.FirstOrDefaultAsync(f => f.Id == id);
        if (function == null)
            return null;

        function.Name = request.Name;
        function.Code = request.Code;
        function.Url = request.Url;
        function.OpenInNewWindow = request.OpenInNewWindow;
        function.Icon = request.Icon;
        function.ParentId = request.ParentId;
        function.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync();

        return MapToDto(function);
    }

    public async Task<bool> SoftDeleteFunctionAsync(int id)
    {
        var function = await _context.Functions.FirstOrDefaultAsync(f => f.Id == id);
        if (function == null)
            return false;

        function.IsDeleted = true;
        function.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteFunctionAsync(int id)
    {
        var function = await _context.Functions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (function == null)
            return false;

        _context.Functions.Remove(function);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Functions.Where(f => f.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(f => f.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<FunctionDto>> GetMenuTreeAsync()
    {
        // 取得所有功能
        var allFunctions = await _context.Functions
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Name)
            .ToListAsync();

        // 建立樹狀結構
        return BuildTree(allFunctions, null);
    }

    public async Task<List<FunctionDto>> GetAllFunctionsAsync()
    {
        return await _context.Functions
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Name)
            .Select(f => MapToDto(f))
            .ToListAsync();
    }

    private static List<FunctionDto> BuildTree(List<Function> allFunctions, int? parentId)
    {
        return allFunctions
            .Where(f => f.ParentId == parentId)
            .Select(f => new FunctionDto(
                f.Id,
                f.Name,
                f.Code,
                f.Url,
                f.OpenInNewWindow,
                f.Icon,
                f.ParentId,
                f.SortOrder,
                f.CreatedAt,
                f.UpdatedAt,
                BuildTree(allFunctions, f.Id)))
            .ToList();
    }

    private static FunctionDto MapToDto(Function function)
    {
        return new FunctionDto(
            function.Id,
            function.Name,
            function.Code,
            function.Url,
            function.OpenInNewWindow,
            function.Icon,
            function.ParentId,
            function.SortOrder,
            function.CreatedAt,
            function.UpdatedAt);
    }
}
