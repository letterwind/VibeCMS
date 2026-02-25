using Microsoft.AspNetCore.Http;
using WebCMS.Core.DTOs.Common;

namespace WebCMS.Infrastructure.Extensions;

/// <summary>
/// 語言查詢參數擴充方法
/// </summary>
public static class LanguageExtensions
{
    /// <summary>
    /// 從接收語言 Header 或查詢參數中提取首選語言代碼
    /// 優先級：查詢參數 > Header > Default (zh-TW)
    /// </summary>
    public static string GetLanguageCodeFromContext(
        this IHttpContextAccessor httpContextAccessor, 
        string? queryLanguage = null)
    {
        // 優先使用查詢參數
        if (!string.IsNullOrWhiteSpace(queryLanguage))
        {
            return queryLanguage;
        }

        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext == null)
        {
            return "zh-TW";
        }

        // 嘗試從查詢字符串獲取
        if (httpContext.Request.Query.TryGetValue("lang", out var langQuery))
        {
            var lang = langQuery.ToString();
            if (!string.IsNullOrWhiteSpace(lang))
            {
                return lang;
            }
        }

        // 嘗試從 Accept-Language Header 獲取
        if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLang))
        {
            var lang = acceptLang.ToString().Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrWhiteSpace(lang))
            {
                // 規範化格式 (如 zh-TW, en-US)
                return lang.Replace("_", "-");
            }
        }

        return "zh-TW";
    }
}
