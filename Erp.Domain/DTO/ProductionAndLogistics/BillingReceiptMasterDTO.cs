using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Billing_Receipt_Master")]
    public class BillingReceiptMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Billing_Receipt_Id { get; set; }
        public Guid Billing_Id { get; set; }
        public string Billing_Number { get; set; } = string.Empty;
        public int Company_Id { get; set; }
        public int Supplier_Id { get; set; }
        [NotMapped]
        public string Supplier_Name { get; set; } = string.Empty;
        public string? Billing_Receipt_Number { get; set; }
        public DateTime? Billing_Receipt_Date { get; set; }
        public decimal Billing_Amount { get; set; }
        public decimal Total_Debit_Quantity { get; set; }
        public decimal Net_Quantity { get; set; }
        public decimal Net_Amount { get; set; }
        [NotMapped]
        public decimal Source_Bill_Qty { get; set; }
        public decimal? Receipt_Amount { get; set; }
        public Guid? payment_Type { get; set; } = Guid.Empty;
        public string? Receipt_Ref_Number { get; set; } = string.Empty;

        [NotMapped]
        public List<BillingReceiptDetailDTO> Billing_Receipt_Details { get; set; } = new();
    }
}