using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;

namespace Erp.Services.Interfaces.Purchase
{
    public interface IPurchaseReceiptService
    {
        Task<string> GenerateNumber();
        Task<List<PurchaseReceiptMasterDTO>> GetReceipts(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate);
        Task<PurchaseReceiptViewModel?> GetReceipt(Guid receiptId);
        Task<List<PurchaseReceiptDetailDTO>> GetPurchaseDetails(Guid purchaseId);
        Task<PurchaseReceiptViewModel> Save(PurchaseReceiptViewModel receipt);
        Task<PurchaseReceiptViewModel> Update(PurchaseReceiptViewModel receipt);
        Task<bool> Delete(Guid receiptId, string? deletedBy);
    }
}
