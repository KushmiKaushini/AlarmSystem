namespace AlarmSystem.Models
{
    public class GroupModel
    {
        public int ID { get; set; }
        public int PARENT_ID { get; set; }
        public string GROUP_NAME { get; set; } = "";
        public string GROUP_TYPE { get; set; } = "";
    }
}