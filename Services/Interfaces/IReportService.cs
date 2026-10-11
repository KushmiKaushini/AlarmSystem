using AlarmSystem.Models.Reports;

namespace AlarmSystem.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<NodeDownReportModel>> GetNodeDownReportAsync(
            DateTime from,
            DateTime to,
            string customer);
    }
}