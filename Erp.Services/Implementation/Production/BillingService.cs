using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class BillingService : IBillingService
    {
        private readonly IBillingRepository _billingRepository;

        public BillingService(IBillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _billingRepository.GenerateNumber();

        public async Task<List<BillingListDTO>> GetBillingList(string? searchText, DateTime? fromDate, DateTime? toDate)
            => await _billingRepository.GetBillingList(searchText, fromDate, toDate);

        public async Task<BillingMasterDTO> GetBilling(Guid billingId)
            => await _billingRepository.GetBilling(billingId);

        public async Task<BillingModel> Save(BillingModel model)
            => await _billingRepository.Save(model);

        public async Task<BillingModel> Update(BillingModel model)
            => await _billingRepository.Update(model);

        public async Task<bool> Delete(Guid billingId)
            => await _billingRepository.Delete(billingId);

        public async Task<BillingReportDTO> GetBillingReport(Guid billingId)
        => await _billingRepository.GetBillingReport(billingId);
    }
}