namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 查詢參數
/// </summary>
public record QueryParameters(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    bool SortDescending = false,
    string? SearchTerm = null,
    bool IncludeDeleted = false
);
