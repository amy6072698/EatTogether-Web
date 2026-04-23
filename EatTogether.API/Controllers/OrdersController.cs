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

        //[HttpGet("Favorites")]
        //[Authorize]
        //public async Task<IActionResult> GetFavorites()
        //{
        //    var memberIdClaim = User.FindFirst("userId")?.Value;
        //    if (!int.TryParse(memberIdClaim, out var memberId))
        //        return Unauthorized();

        //    var favorites = await _service.GetFavoritesAsync(memberId);
        //    return Ok(favorites);
        //}
        // 先這樣
        [HttpGet("Favorites")]
        [AllowAnonymous] // 暫時改成 AllowAnonymous
        public async Task<IActionResult> GetFavorites([FromQuery] int? memberId)
        {
            // 先嘗試從 JWT 取，沒有就用 query parameter（測試用）
            var memberIdClaim = User.FindFirst("userId")?.Value;
            int id;
            if (!int.TryParse(memberIdClaim, out id))
            {
                if (memberId == null) return Unauthorized();
                id = memberId.Value;
            }

            var favorites = await _service.GetFavoritesAsync(id);
            return Ok(favorites);
        }

        // 會員歷史訂單
        [HttpGet("MemberOrderHistory")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMemberOrderHistory([FromQuery] int? memberId)
        {
            var memberIdClaim = User.FindFirst("userId")?.Value;
            int id;
            if (!int.TryParse(memberIdClaim, out id))
            {
                if (memberId == null) return Unauthorized();
                id = memberId.Value;
            }
            var history = await _service.GetMemberOrderHistoryAsync(id);
            return Ok(history);
        }
    }
}