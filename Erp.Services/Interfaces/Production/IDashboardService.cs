using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Services.Interfaces.Production
{
    public interface IDashboardService
    {
        Task<DashboardOverviewDTO> GetOverview(DateTime? fromDate, DateTime? toDate);
    }
}