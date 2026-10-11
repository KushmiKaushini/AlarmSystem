using AlarmSystem.ViewModels;

namespace AlarmSystem.Services.Interfaces
{
    public interface IWarRoomService
    {
        Task<WarRoomDashboardViewModel> GetDashboardAsync();
    }
}