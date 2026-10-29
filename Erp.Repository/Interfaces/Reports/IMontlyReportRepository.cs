using Erp.Domain.DTO.Reports;

namespace Erp.Repository.Interfaces.Reports
{
    public interface IMontlyReportRepository
    {
        Task<MontlyReportResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate);
    }
}
