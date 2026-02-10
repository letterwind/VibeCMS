using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.User;

namespace WebCMS.Core.Interfaces;

/// <summary>
/// 使用者服務介面
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得使用者列表（分頁）
    /// </summary>
    Task<PagedResult<UserDto>> GetUsersAsync(QueryParameters query);

    /// <summary>
    /// 取得單一使用者
    /// </summary>
    Task<UserDto?> GetUserByIdAsync(int id);

    /// <summary>
    /// 建立使用者
    /// </summary>
    Task<UserDto> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// 更新使用者
    /// </summary>
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request);

    /// <summary>
    /// 軟刪除使用者
    /// </summary>
    Task<bool> SoftDeleteUserAsync(int id);

    /// <summary>
    /// 永久刪除使用者（僅限超級管理員）
    /// </summary>
    Task<bool> HardDeleteUserAsync(int id);

    /// <summary>
    /// 檢查帳號是否已存在
    /// </summary>
    Task<bool> IsAccountExistsAsync(string account, int? excludeId = null);

    /// <summary>
    /// 檢查密碼是否過期
    /// </summary>
    Task<bool> IsPasswordExpiredAsync(int userId);

    /// <summary>
    /// 更新使用者密碼
    /// </summary>
    Task<bool> UpdatePasswordAsync(int userId, string newPassword);

    /// <summary>
    /// 驗證帳號格式
    /// </summary>
    ValidationResult ValidateAccount(string account);

    /// <summary>
    /// 驗證密碼格式
    /// </summary>
    ValidationResult ValidatePassword(string password, string account);

    /// <summary>
    /// 驗證新密碼是否與目前密碼相同
    /// </summary>
    Task<bool> IsSameAsCurrentPasswordAsync(int userId, string newPassword);
}
