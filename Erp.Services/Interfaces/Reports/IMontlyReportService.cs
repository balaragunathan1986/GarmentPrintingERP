using Erp.Domain.DTO.Reports;

namespace Erp.Services.Interfaces.Reports
{
    public interface IMontlyReportService
    {
        Task<MontlyReportResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate);
    }
}
