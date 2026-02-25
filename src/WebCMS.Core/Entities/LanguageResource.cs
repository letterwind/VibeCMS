namespace WebCMS.Core.Entities;

/// <summary>
/// 語言資源實體 - 儲存系統介面文字的多語言翻譯
/// 用於管理按鈕、標籤、訊息等系統提示文字
/// </summary>
public class LanguageResource
{
    /// <summary>
    /// 資源 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// </summary>
    public string LanguageCode { get; set; } = "zh-TW";

    /// <summary>
    /// 資源鍵（如 "article.addArticle", "common.save"）
    /// 支持多層級，以 "." 分隔
    /// </summary>
    public string ResourceKey { get; set; } = null!;

    /// <summary>
    /// 資源值（實際的翻譯文字）
    /// </summary>
    public string ResourceValue { get; set; } = null!;

    /// <summary>
    /// 資源類型（Button/Label/Message/Error/Placeholder 等）
    /// </summary>
    public string ResourceType { get; set; } = "Label";

    /// <summary>
    /// 資源說明/備註
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 建立者名稱
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 更新者名稱
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// 是否刪除
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 刪除時間
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
