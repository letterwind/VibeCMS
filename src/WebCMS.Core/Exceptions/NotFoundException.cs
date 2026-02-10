using WebCMS.Core.DTOs.Common;

namespace WebCMS.Core.Exceptions;

/// <summary>
/// 找不到資源例外
/// 當請求的資源不存在時拋出
/// 對應 HTTP 404 Not Found
/// </summary>
public class NotFoundException : BaseException
{
    /// <summary>
    /// 資源類型
    /// </summary>
    public string? ResourceType { get; }

    /// <summary>
    /// 資源識別碼
    /// </summary>
    public object? ResourceId { get; }

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="message">錯誤訊息</param>
    public NotFoundException(string message = "找不到請求的資源") 
        : base(ErrorCodes.ResourceNotFound, message)
    {
    }

    /// <summary>
    /// 建構子（含資源資訊）
    /// </summary>
    /// <param name="resourceType">資源類型</param>
    /// <param name="resourceId">資源識別碼</param>
    public NotFoundException(string resourceType, object resourceId) 
        : base(ErrorCodes.ResourceNotFound, $"找不到 {resourceType}（ID: {resourceId}）")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// 建立找不到使用者的例外
    /// </summary>
    public static NotFoundException User(int userId)
    {
        return new NotFoundException("使用者", userId);
    }

    /// <summary>
    /// 建立找不到角色的例外
    /// </summary>
    public static NotFoundException Role(int roleId)
    {
        return new NotFoundException("角色", roleId);
    }

    /// <summary>
    /// 建立找不到功能的例外
    /// </summary>
    public static NotFoundException Function(int functionId)
    {
        return new NotFoundException("功能", functionId);
    }

    /// <summary>
    /// 建立找不到分類的例外
    /// </summary>
    public static NotFoundException Category(int categoryId)
    {
        return new NotFoundException("分類", categoryId);
    }

    /// <summary>
    /// 建立找不到文章的例外
    /// </summary>
    public static NotFoundException Article(int articleId)
    {
        return new NotFoundException("文章", articleId);
    }
}
