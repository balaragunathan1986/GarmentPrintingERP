namespace Erp.Domain.DTO.ProductionAndLogistics
{
    public class BillingReceiptListDTO
    {
        public int Id { get; set; }
        public Guid Billing_Receipt_Id { get; set; }
        public Guid Billing_Id { get; set; }
        public string Billing_Number { get; set; } = string.Empty;
        public int Supplier_Id { get; set; }
        public string Supplier_Name { get; set; } = string.Empty;
        public string? Billing_Receipt_Number { get; set; }
        public DateTime? Billing_Receipt_Date { get; set; }
        public decimal Billing_Amount { get; set; }
        public decimal Source_Bill_Qty { get; set; }
        public decimal Total_Debit_Quantity { get; set; }
        public decimal Net_Quantity { get; set; }
        public decimal Net_Amount { get; set; }
        public decimal Receipt_Amount { get; set; } = decimal.Zero;
        public string Receipt_ref_Number { get; set; } = string.Empty;
        public string Payment_Type {  get; set; } = string.Empty;
        
    }
}