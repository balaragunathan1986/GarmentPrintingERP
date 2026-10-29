namespace Erp.Domain.DTO.Reports
{
    public class PurchaseSupplierOutstandingResponseDTO
    {
        public string FolderName { get; set; } = "Reports";
        public string ReportName { get; set; } = "PurchaseSupplierOutstanding";
        public List<PurchaseSupplierOutstandingDTO> Rows { get; set; } = new();
    }

    public class PurchaseSupplierOutstandingDTO
    {
        public string Payment_Voucher_Number { get; set; } = string.Empty;
        public DateTime? Payment_Voucher_Date { get; set; }
        public string? BillNo { get; set; }
        public string Supplier_Name { get; set; } = string.Empty;
        public decimal AdvAmount { get; set; }
        public decimal? JobRecAmount { get; set; }
        public decimal BalAmount { get; set; }
        public string Payment_Status { get; set; } = string.Empty;
    }
}