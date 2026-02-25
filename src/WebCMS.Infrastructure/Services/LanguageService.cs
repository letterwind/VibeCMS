using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 語言服務實現
/// </summary>
public class LanguageService : ILanguageService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string LanguageCacheKey = "languages_active";
    private const int CacheDurationMinutes = 60;

    public LanguageService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <summary>
    /// 取得所有啟用的語言
    /// </summary>
    public async Task<IEnumerable<Language>> GetActiveLanguagesAsync()
    {
        if (_cache.TryGetValue(LanguageCacheKey, out var cachedLanguages) &&
            cachedLanguages is IEnumerable<Language> languages)
        {
            return languages;
        }

        var languageList = await _context.Languages
            .Where(l => l.IsActive)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        _cache.Set(LanguageCacheKey, languageList,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                TimeSpan.FromMinutes(CacheDurationMinutes)));

        return languageList;
    }

    /// <summary>
    /// 取得單一語言
    /// </summary>
    public async Task<Language?> GetLanguageByCodeAsync(string languageCode)
    {
        var languages = await GetActiveLanguagesAsync();
        return languages.FirstOrDefault(l => l.LanguageCode == languageCode);
    }

    /// <summary>
    /// 驗證語言代碼是否有效
    /// </summary>
    public async Task<bool> IsValidLanguageCodeAsync(string languageCode)
    {
        var language = await GetLanguageByCodeAsync(languageCode);
        return language != null;
    }

    /// <summary>
    /// 快取清除
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(LanguageCacheKey);
    }
}
