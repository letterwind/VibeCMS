namespace WebCMS.Core.DTOs.Common;

/// <summary>
/// 語言資源 DTO
/// </summary>
public class LanguageResourceDto
{
    public int Id { get; set; }

    public string LanguageCode { get; set; } = "zh-TW";

    public string ResourceKey { get; set; } = null!;

    public string ResourceValue { get; set; } = null!;

    public string ResourceType { get; set; } = "Label";

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }
}

/// <summary>
/// 建立/更新語言資源請求
/// </summary>
public class CreateOrUpdateLanguageResourceRequest
{
    public string LanguageCode { get; set; } = "zh-TW";

    public string ResourceKey { get; set; } = null!;

    public string ResourceValue { get; set; } = null!;

    public string ResourceType { get; set; } = "Label";

    public string? Description { get; set; }
}

/// <summary>
/// 批量更新語言資源請求
/// </summary>
public class BatchUpdateLanguageResourcesRequest
{
    public string LanguageCode { get; set; } = "zh-TW";

    public List<CreateOrUpdateLanguageResourceRequest> Resources { get; set; } = new();
}

/// <summary>
/// 語言檔匯出/匯入 DTO
/// </summary>
public class LanguageFileExportDto
{
    /// <summary>
    /// 語言代碼
    /// </summary>
    public string LanguageCode { get; set; } = null!;

    /// <summary>
    /// ISO 語言名稱
    /// </summary>
    public string LanguageName { get; set; } = null!;

    /// <summary>
    /// 資源字典 - 鍵值對形式
    /// 支持多層級鍵，如：
    /// {
    ///   "common": { "save": "保存", "cancel": "取消" },
    ///   "article": { "addArticle": "新增文章" }
    /// }
    /// </summary>
    public Dictionary<string, object> Resources { get; set; } = new();

    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 語言檔匯入請求
/// </summary>
public class LanguageFileImportRequest
{
    public string LanguageCode { get; set; } = null!;

    /// <summary>
    /// JSON 格式的資源內容
    /// </summary>
    public string FileContent { get; set; } = null!;

    /// <summary>
    /// 是否覆蓋現有資源（true: 覆蓋，false: 合併）
    /// </summary>
    public bool Overwrite { get; set; } = false;
}
