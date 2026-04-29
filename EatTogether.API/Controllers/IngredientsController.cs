using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Text.Json;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IngredientsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;

    private const string Model = "gemini-2.5-flash-lite";

    public IngredientsController(IConfiguration config, IHttpClientFactory httpClientFactory, IMemoryCache cache)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    // POST /api/Ingredients/info
    [HttpPost("info")]
    public async Task<IActionResult> GetIngredientInfo([FromBody] IngredientInfoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.IngredientName))
            return BadRequest(new { error = "ingredientName 不得為空" });

        var cacheKey = $"ingredient:{req.IngredientName.Trim().ToLower()}";
        if (_cache.TryGetValue(cacheKey, out string? cached))
            return Ok(new { text = cached });

        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return StatusCode(503, new { error = "Gemini API Key 未設定" });

        var prompt = $"食材：{req.IngredientName}\n請用繁體中文回傳以下 JSON，禁止任何多餘文字或 markdown：\n{{\"origin\":\"產地(20字內)\",\"cooking\":\"烹調(20字內)\",\"allergy\":\"過敏(20字內)\",\"nutrition\":\"營養(20字內)\"}}";

        var payload = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            },
            generationConfig = new { maxOutputTokens = 120, temperature = 0.0 }
        });

        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(20);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={apiKey}";

        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        var httpRes = await client.PostAsync(url, content);

        if (!httpRes.IsSuccessStatusCode)
        {
            var errorBody = await httpRes.Content.ReadAsStringAsync();
            var sc = (int)httpRes.StatusCode;
            if (sc == 429)
                return StatusCode(429, new { error = "AI 食材查詢請求過於頻繁，請稍候幾秒再試。" });
            if (sc == 503)
                return StatusCode(503, new { error = "Gemini 服務暫時忙碌，請稍後再試。" });
            return StatusCode(502, new { error = $"Gemini API 回傳錯誤 ({sc})", detail = errorBody });
        }

        var json = await httpRes.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        var text = data
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "";

        var cleaned = text.Trim();
        if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.TrimStart('`');
            if (cleaned.StartsWith("json")) cleaned = cleaned[4..];
            cleaned = cleaned.Trim().TrimEnd('`').Trim();
        }

        string result;
        try
        {
            var parsed = JsonSerializer.Deserialize<JsonElement>(cleaned);
            var origin = parsed.GetProperty("origin").GetString() ?? "";
            var cooking = parsed.GetProperty("cooking").GetString() ?? "";
            var allergy = parsed.GetProperty("allergy").GetString() ?? "";
            var nutrition = parsed.GetProperty("nutrition").GetString() ?? "";
            result = $"【來源產地】\n{origin}\n\n【烹煮方式】\n{cooking}\n\n【過敏原提示】\n{allergy}\n\n【營養價值】\n{nutrition}";
        }
        catch
        {
            result = text;
        }

        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
        return Ok(new { text = result });
    }
}

public record IngredientInfoRequest(string IngredientName);
