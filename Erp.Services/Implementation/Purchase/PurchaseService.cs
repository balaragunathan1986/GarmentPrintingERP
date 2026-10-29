using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;
using Erp.Repository.Interfaces.Purchase;
using Erp.Services.Interfaces.Purchase;

namespace Erp.Services.Implementation.Purchase
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;

        public PurchaseService(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public Task<string> GenerateNumber() => _purchaseRepository.GenerateNumber();

        public Task<List<PurchaseMasterListDTO>> GetPurchases(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
            => _purchaseRepository.GetPurchases(searchText, fromDate, toDate);

        public Task<PurchaseViewModel?> GetPurchase(Guid purchaseId)
            => _purchaseRepository.GetPurchase(purchaseId);

        public Task<PurchaseViewModel> Save(PurchaseViewModel purchase)
            => _purchaseRepository.Save(purchase);

        public Task<PurchaseViewModel> Update(PurchaseViewModel purchase)
            => _purchaseRepository.Update(purchase);

        public Task<bool> Delete(Guid purchaseId, string? deletedBy)
            => _purchaseRepository.Delete(purchaseId, deletedBy);
    }
}
