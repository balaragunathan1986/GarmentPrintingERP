using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IBillingReceiptService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<BillingReceiptListDTO>> GetBillingReceiptList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<BillingReceiptMasterDTO> GetBillingReceipt(Guid billingReceiptId);
        Task<BillingReceiptModel> Save(BillingReceiptModel model);
        Task<BillingReceiptModel> Update(BillingReceiptModel model);
        Task<bool> Delete(Guid billingReceiptId);
    }
}