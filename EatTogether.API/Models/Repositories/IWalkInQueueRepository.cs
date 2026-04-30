using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;

namespace EatTogether.API.Models.Repositories;

public interface IWalkInQueueRepository
{
    /// <summary>今日等待中 + 已叫號的候位清單（按登記時間排序）</summary>
    Task<List<WalkInQueue>> GetTodayActiveAsync();

    /// <summary>用電話查今日最新一筆候位（Status IN 0,1,2）</summary>
    Task<WalkInQueue?> GetTodayByPhoneAsync(string phone);

    /// <summary>依 Id 查單筆</summary>
    Task<WalkInQueue?> GetByIdAsync(int id);

    /// <summary>建立候位，回傳新 Id</summary>
    Task<int> CreateAsync(WalkInCreateDto dto, string queueNumber, int? memberId);

    /// <summary>主動放棄候位（Status=3, LeftAt=now）</summary>
    Task LeaveAsync(int id);

    /// <summary>取得今日已使用的最大流水號（用於產生 QueueNumber）</summary>
    Task<int> GetMaxSeqOfDayAsync();
}
