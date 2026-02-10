namespace WebCMS.Core.Entities;

/// <summary>
/// 軟刪除介面，實作此介面的實體將支援軟刪除功能
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
