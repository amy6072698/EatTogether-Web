using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EatTogether.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalkInQueuesController : ControllerBase
{
    private readonly WalkInQueueService _service;

    public WalkInQueuesController(WalkInQueueService service)
    {
        _service = service;
    }

    // GET api/WalkInQueues/TodayStatus
    /// <summary>今日等待中/已叫號 組數摘要（前台候位頁用）</summary>
    [HttpGet("TodayStatus")]
    [AllowAnonymous]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> GetTodayStatus()
    {
        var dto = await _service.GetTodayStatusAsync();
        return Ok(dto);
    }

    // POST api/WalkInQueues
    /// <summary>登記候位</summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> Register([FromBody] WalkInCreateDto dto)
    {
        var result = await _service.RegisterAsync(dto, memberId: null);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Value);
    }

    // GET api/WalkInQueues/My?phone=09xxxxxxxx
    /// <summary>用電話查詢今日候位狀態</summary>
    [HttpGet("My")]
    [AllowAnonymous]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> GetMyStatus([FromQuery] string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "請提供電話號碼" });

        var result = await _service.GetByPhoneAsync(phone);

        if (!result.IsSuccess)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Value);
    }

    // PUT api/WalkInQueues/{id}/Leave
    /// <summary>主動放棄候位</summary>
    [HttpPut("{id:int}/Leave")]
    [AllowAnonymous]
    [EnableRateLimiting("GeneralPolicy")]
    public async Task<IActionResult> Leave(int id)
    {
        var result = await _service.LeaveAsync(id);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "已取消候位" });
    }
}
