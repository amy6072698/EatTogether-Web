using EatTogether.API.Models.EfModels;
using EatTogether.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewsController(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    // GET /api/reviews/{dishId}
    [AllowAnonymous]
    [HttpGet("{dishId:int}")]
    public async Task<IActionResult> GetByDishId(int dishId)
    {
        var reviews = await _reviewRepository.GetByDishIdAsync(dishId);
        var result = reviews.Select(r => new
        {
            id        = r.Id,
            dishId    = r.DishId,
            nickname  = r.Nickname,
            content   = r.Content,
            createdAt = r.CreatedAt
        });
        return Ok(result);
    }

    // POST /api/reviews/{dishId}
    [Authorize]
    [HttpPost("{dishId:int}")]
    public async Task<IActionResult> Add(int dishId, [FromBody] AddReviewRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var nickname = User.FindFirstValue(JwtRegisteredClaimNames.Name)
                    ?? User.FindFirstValue(ClaimTypes.Name)
                    ?? "會員";

        var review = new Review
        {
            DishId    = dishId,
            Nickname  = nickname,
            Content   = request.Content,
            CreatedAt = DateTime.Now
        };

        await _reviewRepository.AddAsync(review);

        return Ok(new
        {
            id        = review.Id,
            dishId    = review.DishId,
            nickname  = review.Nickname,
            content   = review.Content,
            createdAt = review.CreatedAt
        });
    }
}

public class AddReviewRequest
{
    [Required]
    [MaxLength(200)]
    public string Content { get; set; }
}
