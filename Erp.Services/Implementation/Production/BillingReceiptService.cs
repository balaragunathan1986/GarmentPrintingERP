using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class BillingReceiptService : IBillingReceiptService
    {
        private readonly IBillingReceiptRepository _billingReceiptRepository;

        public BillingReceiptService(IBillingReceiptRepository billingReceiptRepository)
        {
            _billingReceiptRepository = billingReceiptRepository;
        }

        public async Task<string> GenerateDocumentNumber() => await _billingReceiptRepository.GenerateNumber();
        public async Task<List<BillingReceiptListDTO>> GetBillingReceiptList(string? searchText, DateTime? fromDate, DateTime? toDate) => await _billingReceiptRepository.GetBillingReceiptList(searchText, fromDate, toDate);
        public async Task<BillingReceiptMasterDTO> GetBillingReceipt(Guid billingReceiptId) => await _billingReceiptRepository.GetBillingReceipt(billingReceiptId);
        public async Task<BillingReceiptModel> Save(BillingReceiptModel model) => await _billingReceiptRepository.Save(model);
        public async Task<BillingReceiptModel> Update(BillingReceiptModel model) => await _billingReceiptRepository.Update(model);
        public async Task<bool> Delete(Guid billingReceiptId) => await _billingReceiptRepository.Delete(billingReceiptId);
    }
}