namespace AlarmSystem.Models
{
    public class ClarityFaultDetailsViewModel
    {
        public ClarityFaultDetail Details { get; set; }
            = new();

        public List<ClarityWorkOrder> WorkOrders { get; set; }
            = new();

        public List<ClarityComment> Comments { get; set; }
            = new();
    }
}