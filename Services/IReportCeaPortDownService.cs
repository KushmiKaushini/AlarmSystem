using AlarmSystem.Models;
using AlarmSystem.Models.ViewModels;

namespace AlarmSystem.Services
{
    public interface IReportCeaPortDownService
    {
        Task<List<ReportCeaPortDown>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string customer);

        Task<List<string>> GetCustomersAsync();
    }
}