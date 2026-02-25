namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 語言 DTO
/// </summary>
public record LanguageDto(
    int Id,
    string LanguageCode,
    string LanguageName,
    bool IsActive,
    int SortOrder
);
