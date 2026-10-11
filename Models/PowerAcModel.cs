namespace AlarmSystem.Models
{
    public class PowerAcModel
    {
        public int ID { get; set; }
        public string NODE { get; set; } = "";
        public int HOURS { get; set; }
        public string FAULT { get; set; } = "";
        public string NW_ENG { get; set; } = "";
        public string ACK_STATUS { get; set; } = "";
        public string ACK_BY { get; set; } = "";
    }
}