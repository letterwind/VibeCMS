namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 分頁結果
/// </summary>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages
);
