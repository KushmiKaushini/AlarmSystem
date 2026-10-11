using AlarmSystem.Models;

namespace AlarmSystem.Services
{
    public interface IAlarmHistoryEngineerService
    {
        Task<List<AlarmHistoryEngineer>> GetMsanAsync(
            DateTime? from,
            DateTime? to,
            string engineer);

        Task<List<AlarmHistoryEngineer>> GetAllAsync(
            DateTime? from,
            DateTime? to,
            string platform,
            string engineer);

        Task<List<string>> GetPlatformsAsync();

        Task<List<string>> GetEngineersAsync();
    }
}