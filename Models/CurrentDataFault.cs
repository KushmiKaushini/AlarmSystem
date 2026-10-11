namespace AlarmSystem.Models
{
    public class CurrentDataFaultModel
    {
        public int SerialNo { get; set; }

        public string DataCircuitID { get; set; }

        public string DetailsOfCustomer { get; set; }

        public DateTime ReportedTime { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string PendingHours { get; set; }
    }
}