using EatTogether.Models.DTOs;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet("Tables")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var tables = await _service.GetTablesAsync();
                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("MenuItems")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMenuItems()
        {
            try
            {
                var items = await _service.GetMenuItemsAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreatePreOrder")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePreOrder([FromBody] CreatePreOrderDto dto)
        {
            try
            {
                var orderNumber = await _service.CreatePreOrderAsync(dto);
                return Ok(new { orderNumber });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var memberIdClaim = User.FindFirst("userId")?.Value;
            if (!int.TryParse(memberIdClaim, out var memberId))
                return Unauthorized();

            var favorites = await _service.GetFavoritesAsync(memberId);
            return Ok(favorites);
        }
    }
}