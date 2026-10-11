namespace AlarmSystem.Models
{
    public class Group
    {
        public long ID { get; set; }

        public string GROUP_NAME { get; set; }

        public string DESCRIPTION { get; set; }

        public long PARENT_ID { get; set; }

        public string PROFILE { get; set; }

        public int? GROUP_ORDER { get; set; }
    }
}