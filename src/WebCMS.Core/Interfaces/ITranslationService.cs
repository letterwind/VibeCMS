using System.Linq.Expressions;
using WebCMS.Core.Entities;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 翻譯服務介面（通用於所有支持多語言的實體）
/// </summary>
public interface ITranslationService<T> where T : BaseEntity
{
    /// <summary>
    /// 取得特定語言的記錄
    /// </summary>
    Task<T?> GetByIdAndLanguageAsync(int id, string languageCode);
    
    /// <summary>
    /// 取得記錄的所有語言版本
    /// </summary>
    Task<IEnumerable<T>> GetAllLanguageVersionsAsync(int id);
    
    /// <summary>
    /// 取得特定語言的列表
    /// </summary>
    Task<IEnumerable<T>> GetByLanguageAsync(string languageCode, Expression<Func<T, bool>>? predicate = null);
    
    /// <summary>
    /// 檢查記錄是否已翻譯為特定語言
    /// </summary>
    Task<bool> IsTranslatedAsync(int id, string languageCode);
    
    /// <summary>
    /// 取得實體的翻譯狀態（哪些語言已翻譯）
    /// </summary>
    Task<IDictionary<string, bool>> GetTranslationStatusAsync(int id);
    
    /// <summary>
    /// 複製翻譯（從一種語言到另一種語言）
    /// </summary>
    Task<T?> CopyTranslationAsync(int sourceId, string sourceLanguage, string targetLanguage);
    
    /// <summary>
    /// 刪除特定語言版本
    /// </summary>
    Task<bool> DeleteLanguageVersionAsync(int id, string languageCode);
}
