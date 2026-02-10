using System.ComponentModel.DataAnnotations;

namespace WebCMS.Core.DTOs.Function;

/// <summary>
/// 建立功能請求 DTO
/// </summary>
public record CreateFunctionRequest(
    [Required(ErrorMessage = "功能名稱為必填欄位")]
    [MaxLength(100, ErrorMessage = "功能名稱最多 100 字元")]
    string Name,

    [Required(ErrorMessage = "功能代碼為必填欄位")]
    [MaxLength(50, ErrorMessage = "功能代碼最多 50 字元")]
    string Code,

    [MaxLength(500, ErrorMessage = "連結網址最多 500 字元")]
    string? Url,

    bool OpenInNewWindow = false,

    [MaxLength(100, ErrorMessage = "圖示名稱最多 100 字元")]
    string? Icon = null,

    int? ParentId = null,

    int SortOrder = 0
);
