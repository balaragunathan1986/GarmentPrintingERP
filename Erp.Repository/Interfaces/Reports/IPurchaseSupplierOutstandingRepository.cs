using Erp.Domain.DTO.Reports;

namespace Erp.Repository.Interfaces.Reports
{
    public interface IPurchaseSupplierOutstandingRepository
    {
        Task<PurchaseSupplierOutstandingResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate);
    }
}