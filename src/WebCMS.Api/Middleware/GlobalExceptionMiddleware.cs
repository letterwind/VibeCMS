using System.Net;
using System.Text.Json;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Exceptions;

namespace WebCMS.Api.Middleware;

/// <summary>
/// 全域例外處理中介軟體
/// 捕獲所有未處理的例外並回傳統一的錯誤回應格式
/// 驗證: 需求 1.6, 3.3
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="next">下一個中介軟體</param>
    /// <param name="logger">日誌記錄器</param>
    /// <param name="environment">環境資訊</param>
    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// 執行中介軟體
    /// </summary>
    /// <param name="context">HTTP 上下文</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Core.Exceptions.ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (UnauthorizedException ex)
        {
            await HandleUnauthorizedExceptionAsync(context, ex);
        }
        catch (ForbiddenException ex)
        {
            await HandleForbiddenExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            await HandleNotFoundExceptionAsync(context, ex);
        }
        catch (BusinessException ex)
        {
            await HandleBusinessExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            await HandleUnexpectedExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 處理驗證例外
    /// HTTP 400 Bad Request
    /// </summary>
    private async Task HandleValidationExceptionAsync(HttpContext context, Core.Exceptions.ValidationException ex)
    {
        _logger.LogWarning(ex, "驗證失敗: {Message}", ex.Message);

        var response = new ErrorResponse(
            Code: ex.ErrorCode,
            Message: ex.Message,
            Errors: ex.Errors.Count > 0 ? ex.Errors : null,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.BadRequest, response);
    }

    /// <summary>
    /// 處理未授權例外
    /// HTTP 401 Unauthorized
    /// 驗證: 需求 1.6 - 不透露具體錯誤原因
    /// </summary>
    private async Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedException ex)
    {
        _logger.LogWarning(ex, "未授權存取: {Message}", ex.Message);

        var response = new ErrorResponse(
            Code: ex.ErrorCode,
            Message: ex.Message,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.Unauthorized, response);
    }

    /// <summary>
    /// 處理禁止存取例外
    /// HTTP 403 Forbidden
    /// 驗證: 需求 3.3 - 拒絕存取並顯示權限不足訊息
    /// </summary>
    private async Task HandleForbiddenExceptionAsync(HttpContext context, ForbiddenException ex)
    {
        _logger.LogWarning(ex, "禁止存取: {Message}", ex.Message);

        var response = new ErrorResponse(
            Code: ex.ErrorCode,
            Message: ex.Message,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.Forbidden, response);
    }

    /// <summary>
    /// 處理找不到資源例外
    /// HTTP 404 Not Found
    /// </summary>
    private async Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
    {
        _logger.LogWarning(ex, "找不到資源: {Message}", ex.Message);

        var response = new ErrorResponse(
            Code: ex.ErrorCode,
            Message: ex.Message,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.NotFound, response);
    }

    /// <summary>
    /// 處理業務邏輯例外
    /// HTTP 422 Unprocessable Entity
    /// </summary>
    private async Task HandleBusinessExceptionAsync(HttpContext context, BusinessException ex)
    {
        _logger.LogWarning(ex, "業務邏輯錯誤: {Message}", ex.Message);

        var response = new ErrorResponse(
            Code: ex.ErrorCode,
            Message: ex.Message,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity, response);
    }

    /// <summary>
    /// 處理未預期的例外
    /// HTTP 500 Internal Server Error
    /// 注意：在生產環境中不暴露敏感資訊
    /// </summary>
    private async Task HandleUnexpectedExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "未預期的錯誤: {Message}", ex.Message);

        // 在生產環境中不暴露詳細錯誤訊息
        var message = _environment.IsDevelopment() 
            ? ex.Message 
            : "發生未預期的錯誤，請稍後再試";

        var response = new ErrorResponse(
            Code: "SYS_001",
            Message: message,
            TraceId: GetTraceId(context)
        );

        await WriteResponseAsync(context, HttpStatusCode.InternalServerError, response);
    }

    /// <summary>
    /// 寫入 HTTP 回應
    /// </summary>
    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, ErrorResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// 取得追蹤 ID
    /// </summary>
    private static string GetTraceId(HttpContext context)
    {
        return context.TraceIdentifier;
    }
}

/// <summary>
/// GlobalExceptionMiddleware 擴充方法
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    /// <summary>
    /// 使用全域例外處理中介軟體
    /// </summary>
    /// <param name="app">應用程式建構器</param>
    /// <returns>應用程式建構器</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
