using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Article;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 文章服務實作
/// </summary>
public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;
    private const int MaxTitleLength = 200;

    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ArticleDto>> GetArticlesAsync(QueryParameters query, int? categoryId = null)
    {
        var queryable = _context.Articles
            .Include(a => a.Category)
            .Include(a => a.Creator)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .AsQueryable();

        // 是否包含已刪除的記錄
        if (query.IncludeDeleted)
        {
            queryable = queryable.IgnoreQueryFilters();
        }

        // 依分類篩選
        if (categoryId.HasValue)
        {
            queryable = queryable.Where(a => a.CategoryId == categoryId.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            queryable = queryable.Where(a =>
                a.Title.ToLower().Contains(searchTerm) ||
                a.Slug.ToLower().Contains(searchTerm) ||
                a.Content.ToLower().Contains(searchTerm));
        }

        // 排序
        queryable = query.SortBy?.ToLower() switch
        {
            "title" => query.SortDescending
                ? queryable.OrderByDescending(a => a.Title)
                : queryable.OrderBy(a => a.Title),
            "category" => query.SortDescending
                ? queryable.OrderByDescending(a => a.Category!.Name)
                : queryable.OrderBy(a => a.Category!.Name),
            "updatedat" => query.SortDescending
                ? queryable.OrderByDescending(a => a.UpdatedAt)
                : queryable.OrderBy(a => a.UpdatedAt),
            _ => queryable.OrderByDescending(a => a.CreatedAt)
        };

        var totalCount = await queryable.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return new PagedResult<ArticleDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize,
            totalPages);
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(int id)
    {
        var article = await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.Creator)
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Id == id);

        return article == null ? null : MapToDto(article);
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleRequest request, int? userId = null)
    {
        // 驗證標題長度
        if (!ValidateArticleTitle(request.Title))
        {
            throw new ArgumentException($"文章標題最多 {MaxTitleLength} 字元");
        }

        // 檢查分類是否存在
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            throw new ArgumentException("指定的分類不存在");
        }

        var article = new Article
        {
            Title = request.Title,
            Content = request.Content,
            Slug = request.Slug,
            CategoryId = request.CategoryId,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        // 處理標籤
        if (request.Tags != null && request.Tags.Count > 0)
        {
            await UpdateArticleTagsAsync(article.Id, request.Tags);
        }

        // 重新載入文章以取得完整資料
        return (await GetArticleByIdAsync(article.Id))!;
    }

    public async Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleRequest request, int? userId = null)
    {
        var article = await _context.Articles
            .Include(a => a.ArticleTags)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            return null;

        // 驗證標題長度
        if (!ValidateArticleTitle(request.Title))
        {
            throw new ArgumentException($"文章標題最多 {MaxTitleLength} 字元");
        }

        // 檢查分類是否存在
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            throw new ArgumentException("指定的分類不存在");
        }

        article.Title = request.Title;
        article.Content = request.Content;
        article.Slug = request.Slug;
        article.CategoryId = request.CategoryId;
        article.MetaTitle = request.MetaTitle;
        article.MetaDescription = request.MetaDescription;
        article.MetaKeywords = request.MetaKeywords;
        article.UpdatedBy = userId;

        // 更新標籤
        await UpdateArticleTagsAsync(article.Id, request.Tags ?? new List<string>());

        await _context.SaveChangesAsync();

        // 重新載入文章以取得完整資料
        return await GetArticleByIdAsync(article.Id);
    }

    public async Task<bool> SoftDeleteArticleAsync(int id)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (article == null)
            return false;

        article.IsDeleted = true;
        article.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteArticleAsync(int id)
    {
        var article = await _context.Articles
            .IgnoreQueryFilters()
            .Include(a => a.ArticleTags)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (article == null)
            return false;

        // 刪除文章標籤關聯
        _context.ArticleTags.RemoveRange(article.ArticleTags);
        _context.Articles.Remove(article);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _context.Articles.Where(a => a.Slug == slug);

        if (excludeId.HasValue)
        {
            query = query.Where(a => a.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public bool ValidateArticleTitle(string title)
    {
        return !string.IsNullOrWhiteSpace(title) && title.Length <= MaxTitleLength;
    }

    public async Task<List<string>> GetAllTagsAsync()
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => t.Name)
            .ToListAsync();
    }

    public async Task<int> GetArticleCountByCategoryAsync(int categoryId)
    {
        return await _context.Articles.CountAsync(a => a.CategoryId == categoryId);
    }

    /// <summary>
    /// 更新文章標籤
    /// </summary>
    private async Task UpdateArticleTagsAsync(int articleId, List<string> tagNames)
    {
        // 移除現有的標籤關聯
        var existingTags = await _context.ArticleTags
            .Where(at => at.ArticleId == articleId)
            .ToListAsync();
        _context.ArticleTags.RemoveRange(existingTags);

        if (tagNames.Count == 0)
        {
            await _context.SaveChangesAsync();
            return;
        }

        // 取得或建立標籤
        foreach (var tagName in tagNames.Distinct())
        {
            var trimmedName = tagName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedName))
                continue;

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == trimmedName);
            if (tag == null)
            {
                tag = new Tag { Name = trimmedName };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            _context.ArticleTags.Add(new ArticleTag
            {
                ArticleId = articleId,
                TagId = tag.Id
            });
        }

        await _context.SaveChangesAsync();
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto(
            article.Id,
            article.Title,
            article.Content,
            article.Slug,
            article.CategoryId,
            article.Category?.Name,
            article.ArticleTags.Select(at => at.Tag?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList(),
            article.MetaTitle,
            article.MetaDescription,
            article.MetaKeywords,
            article.CreatedAt,
            article.UpdatedAt,
            article.CreatedBy,
            article.Creator?.DisplayName
        );
    }
}
