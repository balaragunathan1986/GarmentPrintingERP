using Erp.Domain.DTO.Reports;
using Erp.Repository.Interfaces.Reports;
using Erp.Services.Interfaces.Reports;

namespace Erp.Services.Implementation.Reports
{
    public class MontlyReportService : IMontlyReportService
    {
        private readonly IMontlyReportRepository _repository;

        public MontlyReportService(IMontlyReportRepository repository)
        {
            _repository = repository;
        }

        public Task<MontlyReportResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate)
            => _repository.GetReport(searchText, fromDate, toDate);
    }
}
