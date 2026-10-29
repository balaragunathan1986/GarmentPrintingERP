using Erp.Domain.DTO.ProductionAndLogistics.Purchase;

namespace Erp.Domain.Model.Purchase
{
    public class PurchaseViewModel
    {
        public PurchaseMasterDTO? Purchase_Master { get; set; }
        public List<PurchaseDetailDTO> Purchase_Details { get; set; } = new();
        public List<PurchaseChargeDTO> Purchase_Charges { get; set; } = new();
    }
}
