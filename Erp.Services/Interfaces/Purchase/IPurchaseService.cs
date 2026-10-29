using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;

namespace Erp.Services.Interfaces.Purchase
{
    public interface IPurchaseService
    {
        Task<string> GenerateNumber();
        Task<List<PurchaseMasterListDTO>> GetPurchases(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<PurchaseViewModel?> GetPurchase(Guid purchaseId);
        Task<PurchaseViewModel> Save(PurchaseViewModel purchase);
        Task<PurchaseViewModel> Update(PurchaseViewModel purchase);
        Task<bool> Delete(Guid purchaseId, string? deletedBy);
    }
}
