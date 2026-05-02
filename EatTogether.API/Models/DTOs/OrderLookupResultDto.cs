namespace EatTogether.Models.DTOs
{
    /// <summary>前台訂單查詢回傳結果</summary>
    public class OrderLookupResultDto
    {
        public string OrderNumber   { get; set; } = "";
        public string CustomerName  { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string PickupTime    { get; set; } = "";
        public int    TotalAmount   { get; set; }
        public string Note          { get; set; } = "";   // 整筆備註
        public List<OrderLookupItemDto> Items { get; set; } = new();
    }

    public class OrderLookupItemDto
    {
        public string  ProductName { get; set; } = "";
        public int     Qty         { get; set; }
        public int     UnitPrice   { get; set; }
        public bool    IsSetMeal   { get; set; }
        public string? ItemNote    { get; set; }
        /// <summary>子項目名稱清單（套餐用）</summary>
        public List<string> SubItems { get; set; } = new();
    }
}
