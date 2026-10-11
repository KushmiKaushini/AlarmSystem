using AlarmSystem.Models;

namespace AlarmSystem.Services
{
    public interface IAlarmHistoryRegionService
    {
        Task<List<string>> GetPlatformsAsync();

        Task<List<string>> GetRegionsAsync();

        Task<List<AlarmHistoryRegion>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string region);
    }
}