namespace AlarmSystem.Models
{
    public class WifiFaultOssModel
    {
        public string REF { get; set; } = "";
        public string AP { get; set; } = "";
        public string WORKGROUP { get; set; } = "";
        public string MODEL { get; set; } = "";
        public DateTime? FAULT_TIME { get; set; }
    }
}