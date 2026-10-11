using AlarmSystem.Models;

namespace AlarmSystem.Services
{
    public interface ISummaryService
    {
        Task<List<Summary>> GetPendingSummaryAsync();

        Task<List<PendingAlarm>> GetPendingAlarmsAsync();

        Task<List<Summary>> GetHistorySummaryAsync();

        Task<List<HistoryAlarm>> GetHistoryAlarmsAsync();
    }
}