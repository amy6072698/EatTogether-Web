using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories;

public class WalkInQueueRepository : IWalkInQueueRepository
{
    private readonly EatTogetherDBContext _db;

    public WalkInQueueRepository(EatTogetherDBContext db)
    {
        _db = db;
    }

    public async Task<List<WalkInQueue>> GetTodayActiveAsync()
    {
        var today = DateTime.Today;
        return await _db.WalkInQueues
            .Where(q => q.RegisteredAt >= today && (q.Status == 0 || q.Status == 1))
            .OrderBy(q => q.RegisteredAt)
            .ToListAsync();
    }

    public async Task<WalkInQueue?> GetTodayByPhoneAsync(string phone)
    {
        var today = DateTime.Today;
        return await _db.WalkInQueues
            .Where(q => q.Phone == phone
                     && q.RegisteredAt >= today
                     && q.Status <= 2)          // 0=等待 1=叫號 2=入座（排除3放棄/4過號）
            .OrderByDescending(q => q.RegisteredAt)
            .FirstOrDefaultAsync();
    }

    public async Task<WalkInQueue?> GetByIdAsync(int id)
    {
        return await _db.WalkInQueues.FindAsync(id);
    }

    public async Task<int> CreateAsync(WalkInCreateDto dto, string queueNumber, int? memberId)
    {
        var entity = new WalkInQueue
        {
            QueueNumber   = queueNumber,
            Name          = dto.Name,
            Phone         = dto.Phone,
            AdultsCount   = dto.AdultsCount,
            ChildrenCount = dto.ChildrenCount,
            Status        = 0,
            Remark        = dto.Remark,
            RegisteredAt  = DateTime.Now,
            MemberId      = memberId
        };
        _db.WalkInQueues.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task LeaveAsync(int id)
    {
        var entity = await _db.WalkInQueues.FindAsync(id);
        if (entity == null) return;
        entity.Status = 3;
        entity.LeftAt = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetMaxSeqOfDayAsync()
    {
        var today = DateTime.Today;
        var max = await _db.WalkInQueues
            .Where(q => q.RegisteredAt >= today)
            .CountAsync();   // 今日已有幾筆，直接用筆數當流水號基底
        return max;
    }
}
