using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebCMS.Api.Authorization;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.DTOs.User;
using WebCMS.Core.Interfaces;

namespace WebCMS.Api.Controllers;

/// <summary>
/// 使用者管理控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// 取得使用者列表（分頁）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers([FromQuery] QueryParameters query)
    {
        var result = await _userService.GetUsersAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得單一使用者
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "找不到指定的使用者" });
        }
        return Ok(user);
    }

    /// <summary>
    /// 建立使用者
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        // 驗證帳號格式
        var accountValidation = _userService.ValidateAccount(request.Account);
        if (!accountValidation.IsValid)
        {
            return BadRequest(new { message = string.Join(", ", accountValidation.Errors) });
        }

        // 驗證密碼格式
        var passwordValidation = _userService.ValidatePassword(request.Password, request.Account);
        if (!passwordValidation.IsValid)
        {
            return BadRequest(new { message = string.Join(", ", passwordValidation.Errors) });
        }

        // 檢查帳號是否已存在
        if (await _userService.IsAccountExistsAsync(request.Account))
        {
            return BadRequest(new { message = "帳號已存在" });
        }

        var user = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// 更新使用者
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        // 取得現有使用者
        var existingUser = await _userService.GetUserByIdAsync(id);
        if (existingUser == null)
        {
            return NotFound(new { message = "找不到指定的使用者" });
        }

        // 如果提供了新密碼，驗證密碼格式
        if (!string.IsNullOrEmpty(request.NewPassword))
        {
            var passwordValidation = _userService.ValidatePassword(request.NewPassword, existingUser.Account);
            if (!passwordValidation.IsValid)
            {
                return BadRequest(new { message = string.Join(", ", passwordValidation.Errors) });
            }

            // 檢查新密碼是否與目前密碼相同
            if (await _userService.IsSameAsCurrentPasswordAsync(id, request.NewPassword))
            {
                return BadRequest(new { message = "新密碼不得與目前密碼相同" });
            }
        }

        var user = await _userService.UpdateUserAsync(id, request);
        return Ok(user);
    }

    /// <summary>
    /// 刪除使用者（軟刪除）
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        var result = await _userService.SoftDeleteUserAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的使用者" });
        }
        return NoContent();
    }

    /// <summary>
    /// 永久刪除使用者（僅限超級管理員）
    /// </summary>
    [HttpDelete("{id}/permanent")]
    [SuperAdmin]
    public async Task<ActionResult> HardDeleteUser(int id)
    {
        var result = await _userService.HardDeleteUserAsync(id);
        if (!result)
        {
            return NotFound(new { message = "找不到指定的使用者" });
        }
        return NoContent();
    }

    /// <summary>
    /// 檢查密碼是否過期
    /// </summary>
    [HttpGet("{id}/password-expired")]
    public async Task<ActionResult<bool>> IsPasswordExpired(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "找不到指定的使用者" });
        }

        var isExpired = await _userService.IsPasswordExpiredAsync(id);
        return Ok(new { isExpired });
    }

    /// <summary>
    /// 驗證帳號格式
    /// </summary>
    [HttpPost("validate-account")]
    [AllowAnonymous]
    public ActionResult ValidateAccount([FromBody] ValidateAccountRequest request)
    {
        var result = _userService.ValidateAccount(request.Account);
        return Ok(new { isValid = result.IsValid, errors = result.Errors });
    }

    /// <summary>
    /// 驗證密碼格式
    /// </summary>
    [HttpPost("validate-password")]
    [AllowAnonymous]
    public ActionResult ValidatePassword([FromBody] ValidatePasswordRequest request)
    {
        var result = _userService.ValidatePassword(request.Password, request.Account);
        return Ok(new { isValid = result.IsValid, errors = result.Errors });
    }
}

/// <summary>
/// 驗證帳號請求
/// </summary>
public record ValidateAccountRequest(string Account);

/// <summary>
/// 驗證密碼請求
/// </summary>
public record ValidatePasswordRequest(string Password, string Account);
