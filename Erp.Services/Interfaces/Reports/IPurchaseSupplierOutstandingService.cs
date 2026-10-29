using Erp.Domain.DTO.Reports;

namespace Erp.Services.Interfaces.Reports
{
    public interface IPurchaseSupplierOutstandingService
    {
        Task<PurchaseSupplierOutstandingResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate);
    }
}