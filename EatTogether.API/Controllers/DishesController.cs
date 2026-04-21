using EatTogether.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DishesController : ControllerBase
{
    private readonly IDishRepository _dishRepository;
    private readonly IConfiguration _config;

    public DishesController(IDishRepository dishRepository, IConfiguration config)
    {
        _dishRepository = dishRepository;
        _config = config;
    }

    private string ResolveImageUrl(string dbImageUrl, string dishName)
    {
        if (!string.IsNullOrEmpty(dbImageUrl)) return dbImageUrl;

        // 從設定取得靜態檔案根目錄（MVC 專案的 wwwroot）
        var staticRoot = _config["StaticFilesRoot"] ?? Directory.GetCurrentDirectory();
        var imagesFolder = Path.Combine(staticRoot, "images");

        var safeName = dishName ?? "";
        foreach (var c in Path.GetInvalidFileNameChars())
            safeName = safeName.Replace(c, '_');

        if (System.IO.File.Exists(Path.Combine(imagesFolder, safeName + ".jpg")))
            return "/images/" + safeName + ".jpg";
        if (System.IO.File.Exists(Path.Combine(imagesFolder, safeName + ".png")))
            return "/images/" + safeName + ".png";

        return "";
    }

    // GET /api/Dishes/active
    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var dishes = await _dishRepository.GetAllActiveAsync();
        var result = dishes.Select(d => new
        {
            id              = d.Id,
            dishName        = d.DishName,
            description     = d.Description,
            price           = d.Price,
            categoryId      = d.CategoryId,
            categoryName    = d.CategoryName,
            imageUrl        = ResolveImageUrl(d.ImageUrl, d.DishName),
            isRecommended   = d.IsRecommended,
            isPopular       = d.IsPopular,
            isVegetarian    = d.IsVegetarian,
            spicyLevel      = d.SpicyLevel,
            ingredientsJson = d.IngredientsJson,
            isLimited       = d.IsLimited,
            startDate       = d.StartDate,
            endDate         = d.EndDate,
            averageScore    = d.AverageScore,
            ratingCount     = d.RatingCount,
            stockStatus     = d.StockStatus
        }).ToList();
        return Ok(result);
    }

    // GET /api/Dishes/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var d = await _dishRepository.GetByIdAsync(id);
        if (d == null) return NotFound();
        return Ok(new
        {
            id              = d.Id,
            dishName        = d.DishName,
            description     = d.Description,
            price           = d.Price,
            categoryId      = d.CategoryId,
            categoryName    = d.CategoryName,
            imageUrl        = ResolveImageUrl(d.ImageUrl, d.DishName),
            isRecommended   = d.IsRecommended,
            isPopular       = d.IsPopular,
            isVegetarian    = d.IsVegetarian,
            spicyLevel      = d.SpicyLevel,
            ingredientsJson = d.IngredientsJson,
            isLimited       = d.IsLimited,
            startDate       = d.StartDate,
            endDate         = d.EndDate,
            averageScore    = d.AverageScore,
            ratingCount     = d.RatingCount,
            stockStatus     = d.StockStatus
        });
    }
}
