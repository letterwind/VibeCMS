namespace WebCMS.Core.DTOs.Function;

/// <summary>
/// 功能 DTO
/// </summary>
public record FunctionDto(
    int Id,
    string Name,
    string Code,
    string LanguageCode,
    string? Url,
    bool OpenInNewWindow,
    string? Icon,
    int? ParentId,
    int SortOrder,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<FunctionDto>? Children = null,
    /// <summary>
    /// 翻譯狀態（鍵為語言代碼，值為是否已翻譯）
    /// </summary>
    Dictionary<string, bool>? AvailableLanguages = null
);
