using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace EatTogether.API.Controllers;  //餐點中的食材說明，加入端點

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class IngredientsController : ControllerBase
{
    private readonly IConfiguration _config;

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
            return StatusCode(503, new { error = "Gemini API Key 未設定，請執行 dotnet user-secrets set \"Gemini:ApiKey\" \"你的金鑰\"" });

        var prompt = $"請用繁體中文簡短介紹食材「{req.IngredientName}」，格式如下（每項1~2句）：\n【來源產地】\n【烹煮方式】\n【過敏原提示】\n【營養價值】\n每項控制在30字以內，語氣輕鬆易懂。";

        var payload = JsonSerializer.Serialize(new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        });

        using var client = new HttpClient();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
        var httpRes = await client.PostAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!httpRes.IsSuccessStatusCode)
        {
            var errBody = await httpRes.Content.ReadAsStringAsync();
            return StatusCode(502, new { error = $"Gemini API 回傳錯誤 ({(int)httpRes.StatusCode})", detail = errBody });
        }

        var json = await httpRes.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        var text = data
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        return Ok(new { text });
    }
}

public record IngredientInfoRequest(string IngredientName);
