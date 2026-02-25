using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 通用翻譯服務實現
/// </summary>
public class TranslationService<T> : ITranslationService<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly ILanguageService _languageService;

    public TranslationService(ApplicationDbContext context, ILanguageService languageService)
    {
        _context = context;
        _languageService = languageService;
    }

    /// <summary>
    /// 取得特定語言的記錄
    /// </summary>
    public async Task<T?> GetByIdAndLanguageAsync(int id, string languageCode)
    {
        // 驗證語言
        if (!await _languageService.IsValidLanguageCodeAsync(languageCode))
        {
            return null;
        }

        return await _context.Set<T>()
            .IgnoreQueryFilters()  // 略過軟刪除過濾；我們自己檢查
            .FirstOrDefaultAsync(e => e.Id == id && e.LanguageCode == languageCode 
                && e.IsDeleted == false);
    }

    /// <summary>
    /// 取得記錄的所有語言版本
    /// </summary>
    public async Task<IEnumerable<T>> GetAllLanguageVersionsAsync(int id)
    {
        return await _context.Set<T>()
            .IgnoreQueryFilters()
            .Where(e => e.Id == id && e.IsDeleted == false)
            .ToListAsync();
    }

    /// <summary>
    /// 取得特定語言的列表
    /// </summary>
    public async Task<IEnumerable<T>> GetByLanguageAsync(string languageCode, 
        Expression<Func<T, bool>>? predicate = null)
    {
        // 驗證語言
        if (!await _languageService.IsValidLanguageCodeAsync(languageCode))
        {
            return Enumerable.Empty<T>();
        }

        var query = _context.Set<T>()
            .Where(e => e.LanguageCode == languageCode);

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 檢查記錄是否已翻譯為特定語言
    /// </summary>
    public async Task<bool> IsTranslatedAsync(int id, string languageCode)
    {
        return await _context.Set<T>()
            .IgnoreQueryFilters()
            .AnyAsync(e => e.Id == id && e.LanguageCode == languageCode 
                && e.IsDeleted == false);
    }

    /// <summary>
    /// 取得實體的翻譯狀態（哪些語言已翻譯）
    /// </summary>
    public async Task<IDictionary<string, bool>> GetTranslationStatusAsync(int id)
    {
        var allLanguages = await _languageService.GetActiveLanguagesAsync();
        var translatedVersions = await _context.Set<T>()
            .IgnoreQueryFilters()
            .Where(e => e.Id == id && e.IsDeleted == false)
            .Select(e => e.LanguageCode)
            .ToListAsync();

        var status = new Dictionary<string, bool>();
        foreach (var language in allLanguages)
        {
            status[language.LanguageCode] = translatedVersions.Contains(language.LanguageCode);
        }

        return status;
    }

    /// <summary>
    /// 複製翻譯（從一種語言到另一種語言）
    /// </summary>
    public async Task<T?> CopyTranslationAsync(int sourceId, string sourceLanguage, 
        string targetLanguage)
    {
        // 取得源語言版本
        var sourceEntity = await GetByIdAndLanguageAsync(sourceId, sourceLanguage);
        if (sourceEntity == null)
        {
            return null;
        }

        // 驗證目標語言
        if (!await _languageService.IsValidLanguageCodeAsync(targetLanguage))
        {
            return null;
        }

        // 檢查目標版本是否已存在
        var existingTarget = await GetByIdAndLanguageAsync(sourceId, targetLanguage);
        if (existingTarget != null)
        {
            return existingTarget;
        }

        // 複製實體（建立新記錄）
        var targetEntity = (T)Activator.CreateInstance(typeof(T))!;
        CopyProperties(sourceEntity, targetEntity);
        //targetEntity.Id = sourceId;
        targetEntity.LanguageCode = targetLanguage;
        targetEntity.CreatedAt = DateTime.UtcNow;
        targetEntity.UpdatedAt = DateTime.UtcNow;

        _context.Set<T>().Add(targetEntity);
        await _context.SaveChangesAsync();

        return targetEntity;
    }

    /// <summary>
    /// 刪除特定語言版本
    /// </summary>
    public async Task<bool> DeleteLanguageVersionAsync(int id, string languageCode)
    {
        var entity = await GetByIdAndLanguageAsync(id, languageCode);
        if (entity == null)
        {
            return false;
        }

        // 軟刪除
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 複製對象的屬性（用於翻譯複製）
    /// </summary>
    private static void CopyProperties(T source, T target)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // 跳過 Id 和 LanguageCode（這些由調用者設置）
            if (property.Name == "Id" || property.Name == "LanguageCode" || 
                property.Name == "CreatedAt" || property.Name == "UpdatedAt" ||
                property.Name == "IsDeleted" || property.Name == "DeletedAt")
            {
                continue;
            }

            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }
    }
}
