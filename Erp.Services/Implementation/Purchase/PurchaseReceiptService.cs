using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;
using Erp.Repository.Interfaces.Purchase;
using Erp.Services.Interfaces.Purchase;

namespace Erp.Services.Implementation.Purchase
{
    public class PurchaseReceiptService : IPurchaseReceiptService
    {
        private readonly IPurchaseReceiptRepository _purchaseReceiptRepository;

        public PurchaseReceiptService(IPurchaseReceiptRepository purchaseReceiptRepository)
        {
            _purchaseReceiptRepository = purchaseReceiptRepository;
        }

        public Task<string> GenerateNumber()
            => _purchaseReceiptRepository.GenerateNumber();

        public Task<List<PurchaseReceiptMasterDTO>> GetReceipts(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
            => _purchaseReceiptRepository.GetReceipts(searchText, fromDate, toDate);

        public Task<PurchaseReceiptViewModel?> GetReceipt(Guid receiptId)
            => _purchaseReceiptRepository.GetReceipt(receiptId);

        public Task<List<PurchaseReceiptDetailDTO>> GetPurchaseDetails(Guid purchaseId)
            => _purchaseReceiptRepository.GetPurchaseDetails(purchaseId);

        public Task<PurchaseReceiptViewModel> Save(PurchaseReceiptViewModel receipt)
            => _purchaseReceiptRepository.Save(receipt);

        public Task<PurchaseReceiptViewModel> Update(PurchaseReceiptViewModel receipt)
            => _purchaseReceiptRepository.Update(receipt);

        public Task<bool> Delete(Guid receiptId, string? deletedBy)
            => _purchaseReceiptRepository.Delete(receiptId, deletedBy);
    }
}
