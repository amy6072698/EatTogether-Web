using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IngredientsController : ControllerBase
{
    private readonly IConfiguration _config;

    private const string Model = "gemini-2.5-flash";

    public IngredientsController(IConfiguration config)
    {
        _config = config;
    }

    // POST /api/Ingredients/info
    [HttpPost("info")]
    public async Task<IActionResult> GetIngredientInfo([FromBody] IngredientInfoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.IngredientName))
            return BadRequest(new { error = "ingredientName 不得為空" });

        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            return StatusCode(503, new { error = "Gemini API Key 未設定" });

        var prompt = $"請用繁體中文簡短介紹食材「{req.IngredientName}」，格式如下（每項1~2句）：\n【來源產地】\n【烹煮方式】\n【過敏原提示】\n【營養價值】\n每項控制在30字以內，語氣輕鬆易懂。";

        var payload = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        });

        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={apiKey}";
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        // 503 = Google 端暫時過載，等待後重試（最多 3 次）
        int[] retryDelays = [2000, 5000, 10000];
        string? lastError = null;

        for (int attempt = 0; attempt <= retryDelays.Length; attempt++)
        {
            if (attempt > 0)
                await Task.Delay(retryDelays[attempt - 1]);

            content = new StringContent(payload, Encoding.UTF8, "application/json");
            var httpRes = await client.PostAsync(url, content);

            if (httpRes.IsSuccessStatusCode)
            {
                var json = await httpRes.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<JsonElement>(json);
                var text = data
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                // 去除 Gemini 自行加上的開場白（第一個【之前的內容）
                var firstBracket = text?.IndexOf('【') ?? -1;
                if (firstBracket > 0) text = text![firstBracket..];

                return Ok(new { text });
            }

            lastError = await httpRes.Content.ReadAsStringAsync();
            if ((int)httpRes.StatusCode != 503)
                return StatusCode(502, new { error = $"Gemini API 回傳錯誤 ({(int)httpRes.StatusCode})", detail = lastError });
        }

        return StatusCode(503, new { error = "Gemini 服務暫時忙碌，請稍後再試。", detail = lastError });
    }
}

public record IngredientInfoRequest(string IngredientName);
