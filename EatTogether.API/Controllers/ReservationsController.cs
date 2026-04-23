using EatTogether.Models.DTOs;
using EatTogether.Models.Repositories;
using EatTogether.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly ReservationService    _service;
        private readonly IReservationRepository _repo;

        public ReservationsController(ReservationService service, IReservationRepository repo)
        {
            _service = service;
            _repo    = repo;
        }

        // ─── 取得 MemberId（未登入回 null）────────────────────────────
        private int? GetMemberId()
        {
            var sub = User.FindFirstValue("sub")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }

        // GET api/Reservations/Availability?date=2026-05-01T12:00&adults=2&children=1
        [HttpGet("Availability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(
            [FromQuery] DateTime date,
            [FromQuery] int adults   = 2,
            [FromQuery] int children = 0)
        {
            var result = await _service.GetAvailabilityAsync(date, adults, children);
            return Ok(result);
        }

        // GET api/Reservations/TableAvailability?date=2026-05-01T12:00
        [HttpGet("TableAvailability")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTableAvailability([FromQuery] DateTime date)
        {
            var result = await _service.GetAvailabilityAsync(date, 0, 0);
            return Ok(result.TableTypeAvailability);
        }

        // POST api/Reservations
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] ReservationCreateDto dto)
        {
            var memberId = GetMemberId();
            var result   = await _service.CreateAsync(dto, memberId);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new
            {
                bookingNumber = result.Value,
                message       = "訂位成功！確認信已寄至您的 Email"
            });
        }

        // GET api/Reservations/My  [需登入]
        [HttpGet("My")]
        [Authorize]
        public async Task<IActionResult> GetMyReservations()
        {
            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var list = await _repo.GetByMemberIdAsync(memberId.Value);
            return Ok(list);
        }

        // GET api/Reservations/Query?bookingNumber=R260311001&email=test@test.com
        [HttpGet("Query")]
        [AllowAnonymous]
        public async Task<IActionResult> QueryGuest(
            [FromQuery] string bookingNumber,
            [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(bookingNumber) || string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "請填寫訂位單號與 Email" });

            var detail = await _repo.GetByBookingNumberAndEmailAsync(bookingNumber, email);
            if (detail == null)
                return NotFound(new { message = "找不到訂位紀錄，請確認訂位單號與 Email" });

            return Ok(detail);
        }

        // PUT api/Reservations/{id}/Cancel
        [HttpPut("{id:int}/Cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel(int id, [FromBody] ReservationCancelDto dto)
        {
            var memberId = GetMemberId();

            EatTogether.Models.Infra.Result result;

            if (memberId.HasValue)
            {
                // 登入會員取消
                result = await _service.CancelByMemberAsync(id, memberId.Value);
            }
            else
            {
                // 訪客取消（需帶 BookingNumber + Email）
                if (string.IsNullOrWhiteSpace(dto.BookingNumber) || string.IsNullOrWhiteSpace(dto.Email))
                    return BadRequest(new { message = "訪客取消需提供訂位單號與 Email" });

                result = await _service.CancelByGuestAsync(dto.BookingNumber!, dto.Email!);
            }

            if (!result.IsSuccess)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "訂位已取消，取消確認信已寄出" });
        }
    }
}
