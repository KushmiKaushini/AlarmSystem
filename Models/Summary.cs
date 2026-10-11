namespace AlarmSystem.Models
{
    public class Summary
    {
        public string Platform { get; set; }

        public int LessThan4Hours { get; set; }

        public int LessThan8Hours { get; set; }

        public int MoreThan8Hours { get; set; }

        public int Total { get; set; }
    }
}