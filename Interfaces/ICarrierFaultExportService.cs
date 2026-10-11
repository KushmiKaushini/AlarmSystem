using AlarmSystem.Models;

namespace AlarmSystem.Services.Interfaces
{
    public interface ICarrierFaultExportService
    {
        Task<List<CarrierFaultExportModel>> GetCarrierFaultsAsync();
    }
}