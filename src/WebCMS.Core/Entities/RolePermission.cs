namespace WebCMS.Core.Entities;

/// <summary>
/// 角色權限實體 - 定義角色對功能的 CRUD 權限
/// 支持按語言區分權限
/// </summary>
public class RolePermission
{
    /// <summary>
    /// 角色 ID
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// 功能 ID
    /// </summary>
    public int FunctionId { get; set; }

    /// <summary>
    /// 語言代碼（如 zh-TW、en-US、ja-JP）
    /// 複合主鍵的一部分，允許同一角色在不同語言有不同權限
    /// </summary>
    public string LanguageCode { get; set; } = "zh-TW";

    /// <summary>
    /// 是否可新增
    /// </summary>
    public bool CanCreate { get; set; }

    /// <summary>
    /// 是否可讀取
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// 是否可更新
    /// </summary>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// 是否可刪除
    /// </summary>
    public bool CanDelete { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 角色（導覽屬性）
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// 功能（導覽屬性）
    /// </summary>
    public Function Function { get; set; } = null!;
}
