using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebCMS.Core.DTOs.Common;
using WebCMS.Core.Entities;
using WebCMS.Core.Interfaces;
using WebCMS.Infrastructure.Data;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 語言資源服務實現
/// 用於管理系統介面文字的多語言翻譯
/// </summary>
public class LanguageResourceService : ILanguageResourceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILanguageService _languageService;

    public LanguageResourceService(ApplicationDbContext context, ILanguageService languageService)
    {
        _context = context;
        _languageService = languageService;
    }

    public async Task<List<LanguageResourceDto>> GetResourcesByLanguageAsync(string languageCode)
    {
        var resources = await _context.LanguageResources
            .Where(r => r.LanguageCode == languageCode && !r.IsDeleted)
            .OrderBy(r => r.ResourceKey)
            .Select(r => new LanguageResourceDto
            {
                Id = r.Id,
                LanguageCode = r.LanguageCode,
                ResourceKey = r.ResourceKey,
                ResourceValue = r.ResourceValue,
                ResourceType = r.ResourceType,
                Description = r.Description,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                CreatedBy = r.CreatedBy,
                UpdatedBy = r.UpdatedBy
            })
            .ToListAsync();

        return resources;
    }

    public async Task<LanguageResourceDto?> GetResourceByKeyAsync(string languageCode, string resourceKey)
    {
        var resource = await _context.LanguageResources
            .FirstOrDefaultAsync(r => r.LanguageCode == languageCode
                && r.ResourceKey == resourceKey
                && !r.IsDeleted);

        if (resource == null)
        {
            return null;
        }

        return new LanguageResourceDto
        {
            Id = resource.Id,
            LanguageCode = resource.LanguageCode,
            ResourceKey = resource.ResourceKey,
            ResourceValue = resource.ResourceValue,
            ResourceType = resource.ResourceType,
            Description = resource.Description,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
            CreatedBy = resource.CreatedBy,
            UpdatedBy = resource.UpdatedBy
        };
    }

    public async Task<LanguageResourceDto> CreateResourceAsync(CreateOrUpdateLanguageResourceRequest request)
    {
        // 驗證語言
        if (!await _languageService.IsValidLanguageCodeAsync(request.LanguageCode))
        {
            throw new ArgumentException($"Invalid language code: {request.LanguageCode}");
        }

        // 檢查是否已存在
        var existing = await _context.LanguageResources
            .FirstOrDefaultAsync(r => r.LanguageCode == request.LanguageCode
                && r.ResourceKey == request.ResourceKey);

        if (existing != null && !existing.IsDeleted)
        {
            throw new InvalidOperationException($"Resource key '{request.ResourceKey}' already exists for language '{request.LanguageCode}'");
        }

        var resource = new LanguageResource
        {
            LanguageCode = request.LanguageCode,
            ResourceKey = request.ResourceKey,
            ResourceValue = request.ResourceValue,
            ResourceType = request.ResourceType,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.LanguageResources.Add(resource);
        await _context.SaveChangesAsync();

        return new LanguageResourceDto
        {
            Id = resource.Id,
            LanguageCode = resource.LanguageCode,
            ResourceKey = resource.ResourceKey,
            ResourceValue = resource.ResourceValue,
            ResourceType = resource.ResourceType,
            Description = resource.Description,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
            CreatedBy = resource.CreatedBy,
            UpdatedBy = resource.UpdatedBy
        };
    }

    public async Task<List<LanguageResourceDto>> BatchCreateOrUpdateResourcesAsync(BatchUpdateLanguageResourcesRequest request)
    {
        // 驗證語言
        if (!await _languageService.IsValidLanguageCodeAsync(request.LanguageCode))
        {
            throw new ArgumentException($"Invalid language code: {request.LanguageCode}");
        }

        var results = new List<LanguageResourceDto>();

        foreach (var resourceReq in request.Resources)
        {
            resourceReq.LanguageCode = request.LanguageCode;

            var existing = await _context.LanguageResources
                .FirstOrDefaultAsync(r => r.LanguageCode == request.LanguageCode
                    && r.ResourceKey == resourceReq.ResourceKey);

            if (existing != null && !existing.IsDeleted)
            {
                // 更新
                existing.ResourceValue = resourceReq.ResourceValue;
                existing.ResourceType = resourceReq.ResourceType;
                existing.Description = resourceReq.Description;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // 建立新的
                var resource = new LanguageResource
                {
                    LanguageCode = request.LanguageCode,
                    ResourceKey = resourceReq.ResourceKey,
                    ResourceValue = resourceReq.ResourceValue,
                    ResourceType = resourceReq.ResourceType,
                    Description = resourceReq.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LanguageResources.Add(resource);
            }
        }

        await _context.SaveChangesAsync();

        // 返回更新後的資源列表
        return await GetResourcesByLanguageAsync(request.LanguageCode);
    }

    public async Task<LanguageResourceDto> UpdateResourceAsync(int id, CreateOrUpdateLanguageResourceRequest request)
    {
        var resource = await _context.LanguageResources.FindAsync(id);
        if (resource == null || resource.IsDeleted)
        {
            throw new ArgumentException($"Resource not found: {id}");
        }

        resource.ResourceValue = request.ResourceValue;
        resource.ResourceType = request.ResourceType;
        resource.Description = request.Description;
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new LanguageResourceDto
        {
            Id = resource.Id,
            LanguageCode = resource.LanguageCode,
            ResourceKey = resource.ResourceKey,
            ResourceValue = resource.ResourceValue,
            ResourceType = resource.ResourceType,
            Description = resource.Description,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
            CreatedBy = resource.CreatedBy,
            UpdatedBy = resource.UpdatedBy
        };
    }

    public async Task<bool> DeleteResourceAsync(int id)
    {
        var resource = await _context.LanguageResources.FindAsync(id);
        if (resource == null || resource.IsDeleted)
        {
            return false;
        }

        resource.IsDeleted = true;
        resource.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteLanguageResourcesAsync(string languageCode)
    {
        var resources = await _context.LanguageResources
            .Where(r => r.LanguageCode == languageCode && !r.IsDeleted)
            .ToListAsync();

        foreach (var resource in resources)
        {
            resource.IsDeleted = true;
            resource.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return resources.Count;
    }

    public async Task<LanguageFileExportDto> ExportResourcesAsync(string languageCode)
    {
        var language = await _languageService.GetLanguageByCodeAsync(languageCode);
        if (language == null)
        {
            throw new ArgumentException($"Invalid language code: {languageCode}");
        }

        var resources = await GetResourcesByLanguageAsync(languageCode);
        var resourceDict = BuildHierarchicalDictionary(resources);

        return new LanguageFileExportDto
        {
            LanguageCode = languageCode,
            LanguageName = language.LanguageName,
            Resources = resourceDict,
            ExportedAt = DateTime.UtcNow
        };
    }

    public async Task<int> ImportResourcesAsync(string languageCode, string fileContent, bool overwrite = false)
    {
        var (isValid, errorMessage) = await ValidateLanguageFileAsync(fileContent);
        if (!isValid)
        {
            throw new ArgumentException($"Invalid language file format: {errorMessage}");
        }

        // 解析 JSON
        var jsonDoc = JsonDocument.Parse(fileContent);
        var importedResources = new List<LanguageResource>();

        if (overwrite)
        {
            await DeleteLanguageResourcesAsync(languageCode);
        }

        ProcessJsonElement(jsonDoc.RootElement, languageCode, "", importedResources);

        _context.LanguageResources.AddRange(importedResources);
        await _context.SaveChangesAsync();

        return importedResources.Count;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateLanguageFileAsync(string fileContent)
    {
        try
        {
            JsonDocument.Parse(fileContent);
            return (true, null);
        }
        catch (JsonException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task ClearCacheAsync(string languageCode)
    {
        // 實現快取清除邏輯（如果使用快取的話）
        await Task.CompletedTask;
    }

    /// <summary>
    /// 構建分層字典（鍵值結構）
    /// </summary>
    private static Dictionary<string, object> BuildHierarchicalDictionary(List<LanguageResourceDto> resources)
    {
        var dict = new Dictionary<string, object>();

        foreach (var resource in resources)
        {
            var parts = resource.ResourceKey.Split('.');
            var current = (Dictionary<string, object>)dict;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.ContainsKey(parts[i]))
                {
                    current[parts[i]] = new Dictionary<string, object>();
                }

                current = (Dictionary<string, object>)current[parts[i]];
            }

            current[parts[^1]] = resource.ResourceValue;
        }

        return dict;
    }

    /// <summary>
    /// 遞歸處理 JSON 元素並轉換為平面結構
    /// </summary>
    private static void ProcessJsonElement(JsonElement element, string languageCode, string prefix, List<LanguageResource> resources)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    ProcessJsonElement(property.Value, languageCode, key, resources);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    resources.Add(new LanguageResource
                    {
                        LanguageCode = languageCode,
                        ResourceKey = key,
                        ResourceValue = property.Value.GetString() ?? "",
                        ResourceType = "Label",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }
    }
}
