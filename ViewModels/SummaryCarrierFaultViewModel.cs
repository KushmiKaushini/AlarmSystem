using AlarmSystem.Models;
using System.Collections.Generic;

namespace AlarmSystem.ViewModels
{
    public class SummaryCarrierFaultViewModel
    {
        public CarrierSummaryModel Summary { get; set; } = new CarrierSummaryModel();

        public List<SummaryCarrierFaultModel> Details { get; set; }
            = new List<SummaryCarrierFaultModel>();
    }
}