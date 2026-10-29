using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardOverviewDTO> GetOverview(DateTime? fromDate, DateTime? toDate)
        {
            return await _dashboardRepository.GetOverview(fromDate, toDate);
        }
    }
}