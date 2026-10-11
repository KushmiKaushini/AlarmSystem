using AlarmSystem.Models;

namespace AlarmSystem.ViewModels
{
    public class FaultDetailsViewModel
    {
        public FaultDetail Detail { get; set; }

        public List<FaultWorkOrder> WorkOrders { get; set; }
            = new();

        public List<FaultComment> Comments { get; set; }
            = new();
    }
}