using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Interfaces;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 語言檔服務實現
/// 支持從資料庫或靜態檔案載入語言資源
/// 優先級：資料庫 → 靜態資源
/// </summary>
public class LanguageFileService : ILanguageFileService
{
    private readonly ILanguageResourceService _languageResourceService;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private const string CACHE_KEY_PREFIX = "LanguageFile_";
    private const int CACHE_DURATION = 300; // 5 分鐘

    public LanguageFileService(
        ILanguageResourceService languageResourceService,
        IMemoryCache cache,
        IConfiguration configuration)
    {
        _languageResourceService = languageResourceService;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<LanguageFileExportDto> GetLanguageFileAsync(string languageCode)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{languageCode}";

        // 從快取取得
        if (_cache.TryGetValue(cacheKey, out LanguageFileExportDto? cachedFile))
        {
            return cachedFile!;
        }

        // 首先嘗試從資料庫載入
        var allowDatabase = _configuration.GetSection("LanguageSettings:AllowDatabaseResources").Value != "false";
        if (allowDatabase)
        {
            try
            {
                var resourceFile = await _languageResourceService.ExportResourcesAsync(languageCode);
                SetCache(cacheKey, resourceFile);
                return resourceFile;
            }
            catch
            {
                // 資料庫載入失敗，繼續嘗試靜態檔案
            }
        }

        // 回退到靜態檔案
        var allowStatic = _configuration.GetSection("LanguageSettings:AllowStaticResources").Value != "false";
        if (allowStatic)
        {
            var staticFile = TryLoadStaticLanguageFile(languageCode);
            if (staticFile != null)
            {
                SetCache(cacheKey, staticFile);
                return staticFile;
            }
        }

        // 如果都找不到，返回空檔案
        return new LanguageFileExportDto
        {
            LanguageCode = languageCode,
            LanguageName = languageCode,
            Resources = new Dictionary<string, object>(),
            ExportedAt = DateTime.UtcNow
        };
    }

    public async Task SaveLanguageFileAsync(string languageCode, LanguageFileExportDto fileData)
    {
        var flatResources = FlattenDictionary(fileData.Resources, "");
        var request = new BatchUpdateLanguageResourcesRequest
        {
            LanguageCode = languageCode,
            Resources = flatResources.Select(kvp => new CreateOrUpdateLanguageResourceRequest
            {
                LanguageCode = languageCode,
                ResourceKey = kvp.Key,
                ResourceValue = kvp.Value,
                ResourceType = "Label"
            }).ToList()
        };

        await _languageResourceService.BatchCreateOrUpdateResourcesAsync(request);

        // 清除快取
        var cacheKey = $"{CACHE_KEY_PREFIX}{languageCode}";
        _cache.Remove(cacheKey);
    }

    public (bool IsValid, string? ErrorMessage) ValidateLanguageFile(string fileContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(fileContent);
            return (true, null);
        }
        catch (JsonException ex)
        {
            return (false, $"Invalid JSON: {ex.Message}");
        }
    }

    public async Task<List<(DateTime Timestamp, string Version)>> GetLanguageFileHistoryAsync(string languageCode)
    {
        // 可選功能：版本管理
        // 目前返回空列表
        await Task.CompletedTask;
        return new List<(DateTime, string)>();
    }

    public async Task ClearCacheAsync(string languageCode)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{languageCode}";
        _cache.Remove(cacheKey);
        await Task.CompletedTask;
    }

    /// <summary>
    /// 嘗試從靜態檔案載入語言資源
    /// </summary>
    private LanguageFileExportDto? TryLoadStaticLanguageFile(string languageCode)
    {
        try
        {
            var statePath = _configuration.GetSection("LanguageSettings:StaticResourcePath").Value ?? "assets/i18n";
            var filePath = Path.Combine(System.AppContext.BaseDirectory, "wwwroot", statePath, $"{languageCode}.json");

            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = File.ReadAllText(filePath);
            var doc = JsonDocument.Parse(content);

            var resources = new Dictionary<string, object>();
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                resources[property.Name] = JsonConvert.ToObject(property.Value) ?? "";
            }

            return new LanguageFileExportDto
            {
                LanguageCode = languageCode,
                LanguageName = languageCode,
                Resources = resources,
                ExportedAt = File.GetLastWriteTimeUtc(filePath)
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 平坦化分層字典
    /// </summary>
    private static List<KeyValuePair<string, string>> FlattenDictionary(Dictionary<string, object> dict, string prefix)
    {
        var result = new List<KeyValuePair<string, string>>();

        foreach (var kvp in dict)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is Dictionary<string, object> nestedDict)
            {
                result.AddRange(FlattenDictionary(nestedDict, key));
            }
            else if (kvp.Value is JsonElement element)
            {
                result.Add(new KeyValuePair<string, string>(key, element.GetRawText()));
            }
            else
            {
                result.Add(new KeyValuePair<string, string>(key, kvp.Value?.ToString() ?? ""));
            }
        }

        return result;
    }

    /// <summary>
    /// 設置快取
    /// </summary>
    private void SetCache(string cacheKey, LanguageFileExportDto value)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CACHE_DURATION)
        };

        _cache.Set(cacheKey, value, cacheOptions);
    }
}

/// <summary>
/// JSON 轉換工具（簡化版）
/// </summary>
internal static class JsonConvert
{
    public static object? ToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object =>
                element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ToObject(p.Value)),
            JsonValueKind.Array =>
                element.EnumerateArray()
                    .Select(e => ToObject(e))
                    .ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number =>
                element.TryGetDouble(out double d) ? d : element.GetInt64(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };
    }
}
