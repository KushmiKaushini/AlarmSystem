using AlarmSystem.Models;
using AlarmSystem.Models.ViewModels;

namespace AlarmSystem.Services
{
    public interface IRpbSummaryService
    {
        Task<RpbSummaryViewModel> GetSummaryAsync();
    }
}