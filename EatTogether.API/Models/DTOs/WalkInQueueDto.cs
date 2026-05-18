namespace EatTogether.API.Models.DTOs;

/// <summary>前台登記候位請求</summary>
public class WalkInCreateDto
{
    public string Name        { get; set; }
    public string Phone       { get; set; }
    public int    AdultsCount  { get; set; }
    public int    ChildrenCount { get; set; }
    public string Remark      { get; set; }
}

/// <summary>候位狀態查詢回傳</summary>
public class WalkInStatusDto
{
    public int      Id           { get; set; }
    public string   QueueNumber  { get; set; }
    public string   Name         { get; set; }
    public int      AdultsCount  { get; set; }
    public int      ChildrenCount { get; set; }
    public int      Status       { get; set; }
    public string   StatusText   { get; set; }
    public DateTime RegisteredAt { get; set; }
    /// <summary>前面還有幾組（Status=0 才有意義）</summary>
    public int GroupsAhead { get; set; }
}

/// <summary>今日候位整體摘要</summary>
public class WalkInTodayStatusDto
{
    /// <summary>等待中組數（Status=0）</summary>
    public int WaitingCount { get; set; }
    /// <summary>已叫號組數（Status=1）</summary>
    public int CalledCount  { get; set; }
}
