using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Repository.Interfaces.Production
{
    public interface IDashboardRepository
    {
        Task<DashboardOverviewDTO> GetOverview(DateTime? fromDate, DateTime? toDate);
    }
}