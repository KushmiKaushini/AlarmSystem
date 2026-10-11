using System.Text.Json;

namespace AlarmSystem.Models
{
    public class ApiTestResponse
    {
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public DateTime Timestamp { get; set; }
        public JsonElement? Data { get; set; }
        public string RawBody { get; set; } = string.Empty;
    }
}
