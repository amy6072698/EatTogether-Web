using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EatTogether.Models.DTOs
{
    public class OrderNoteDto
    {
        [JsonPropertyName("order")]
        public string? Order { get; set; }

        [JsonPropertyName("items")]
        public Dictionary<string, string> Items { get; set; } = new();

        // 外帶訂單顧客資訊（存入 Note JSON）
        [JsonPropertyName("customerName")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("customerPhone")]
        public string? CustomerPhone { get; set; }

        [JsonPropertyName("pickupTime")]
        public string? PickupTime { get; set; }
    }
}
