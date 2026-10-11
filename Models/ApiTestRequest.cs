namespace AlarmSystem.Models
{
    public class ApiTestRequest
    {
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public string BearerToken { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
    }
}
