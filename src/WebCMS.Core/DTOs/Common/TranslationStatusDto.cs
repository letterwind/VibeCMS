namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 翻譯狀態 DTO
/// </summary>
public record TranslationStatusDto(
    int EntityId,
    string EntityType,
    Dictionary<string, bool> LanguageStatus,
    /// <summary>
    /// 完成百分率 (0-100)
    /// </summary>
    int CompletionPercentage
);
