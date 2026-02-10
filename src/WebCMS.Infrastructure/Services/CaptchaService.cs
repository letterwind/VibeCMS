using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using SkiaSharp;
using WebCMS.Core.DTOs.Auth;
using WebCMS.Core.Interfaces;

namespace WebCMS.Infrastructure.Services;

/// <summary>
/// 驗證碼服務實作
/// </summary>
public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;
    private const int CaptchaLength = 4;
    private const int ImageWidth = 120;
    private const int ImageHeight = 40;
    private const int CaptchaExpirationMinutes = 5;
    private const string CaptchaChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public CaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public CaptchaResponse Generate()
    {
        var captchaText = GenerateCaptchaText();
        var token = GenerateToken();
        var imageBase64 = GenerateCaptchaImage(captchaText);

        // 將驗證碼存入快取
        _cache.Set(
            GetCacheKey(token),
            captchaText.ToUpperInvariant(),
            TimeSpan.FromMinutes(CaptchaExpirationMinutes));

        return new CaptchaResponse(imageBase64, token);
    }

    public bool Validate(string captcha, string token)
    {
        if (string.IsNullOrWhiteSpace(captcha) || string.IsNullOrWhiteSpace(token))
            return false;

        var cacheKey = GetCacheKey(token);
        if (!_cache.TryGetValue(cacheKey, out string? storedCaptcha))
            return false;

        // 驗證後移除快取（一次性使用）
        _cache.Remove(cacheKey);

        return string.Equals(captcha.Trim(), storedCaptcha, StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateCaptchaText()
    {
        var chars = new char[CaptchaLength];
        for (int i = 0; i < CaptchaLength; i++)
        {
            chars[i] = CaptchaChars[RandomNumberGenerator.GetInt32(CaptchaChars.Length)];
        }
        return new string(chars);
    }

    private static string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GetCacheKey(string token) => $"captcha:{token}";

    private static string GenerateCaptchaImage(string text)
    {
        using var surface = SKSurface.Create(new SKImageInfo(ImageWidth, ImageHeight));
        var canvas = surface.Canvas;

        // 背景
        canvas.Clear(SKColors.White);

        // 繪製干擾線
        DrawNoiseLines(canvas);

        // 繪製文字
        DrawText(canvas, text);

        // 繪製干擾點
        DrawNoiseDots(canvas);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return Convert.ToBase64String(data.ToArray());
    }

    private static void DrawNoiseLines(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true,
            StrokeWidth = 1
        };

        for (int i = 0; i < 5; i++)
        {
            paint.Color = new SKColor(
                (byte)RandomNumberGenerator.GetInt32(100, 200),
                (byte)RandomNumberGenerator.GetInt32(100, 200),
                (byte)RandomNumberGenerator.GetInt32(100, 200));

            canvas.DrawLine(
                RandomNumberGenerator.GetInt32(ImageWidth),
                RandomNumberGenerator.GetInt32(ImageHeight),
                RandomNumberGenerator.GetInt32(ImageWidth),
                RandomNumberGenerator.GetInt32(ImageHeight),
                paint);
        }
    }

    private static void DrawText(SKCanvas canvas, string text)
    {
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 24);
        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        float x = 10;
        foreach (char c in text)
        {
            paint.Color = new SKColor(
                (byte)RandomNumberGenerator.GetInt32(0, 100),
                (byte)RandomNumberGenerator.GetInt32(0, 100),
                (byte)RandomNumberGenerator.GetInt32(0, 100));

            canvas.DrawText(c.ToString(), x, 28, SKTextAlign.Left, font, paint);
            x += 25;
        }
    }

    private static void DrawNoiseDots(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        for (int i = 0; i < 50; i++)
        {
            paint.Color = new SKColor(
                (byte)RandomNumberGenerator.GetInt32(0, 255),
                (byte)RandomNumberGenerator.GetInt32(0, 255),
                (byte)RandomNumberGenerator.GetInt32(0, 255));

            canvas.DrawPoint(
                RandomNumberGenerator.GetInt32(ImageWidth),
                RandomNumberGenerator.GetInt32(ImageHeight),
                paint);
        }
    }
}
