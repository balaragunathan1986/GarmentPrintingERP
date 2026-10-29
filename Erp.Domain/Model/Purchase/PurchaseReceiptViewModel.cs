using Erp.Domain.DTO.ProductionAndLogistics.Purchase;

namespace Erp.Domain.Model.Purchase
{
    public class PurchaseReceiptViewModel
    {
        public PurchaseReceiptMasterDTO? PurchaseReceipt_Master { get; set; }
        public List<PurchaseReceiptDetailDTO> PurchaseReceipt_Details { get; set; } = new();
    }
}
