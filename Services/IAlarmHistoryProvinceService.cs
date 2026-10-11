using AlarmSystem.Models;

namespace AlarmSystem.Services
{
    public interface IAlarmHistoryProvinceService
    {
        Task<List<string>> GetPlatformsAsync();

        Task<List<string>> GetProvincesAsync();

        Task<List<AlarmHistoryProvince>> GetReportAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string province);
    }
}