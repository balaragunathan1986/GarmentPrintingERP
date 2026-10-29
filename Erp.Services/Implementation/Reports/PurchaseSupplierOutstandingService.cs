using Erp.Domain.DTO.Reports;
using Erp.Repository.Interfaces.Reports;
using Erp.Services.Interfaces.Reports;

namespace Erp.Services.Implementation.Reports
{
    public class PurchaseSupplierOutstandingService : IPurchaseSupplierOutstandingService
    {
        private readonly IPurchaseSupplierOutstandingRepository _repository;

        public PurchaseSupplierOutstandingService(IPurchaseSupplierOutstandingRepository repository)
        {
            _repository = repository;
        }

        public Task<PurchaseSupplierOutstandingResponseDTO> GetReport(string? searchText, DateTime? fromDate, DateTime? toDate)
            => _repository.GetReport(searchText, fromDate, toDate);
    }
}