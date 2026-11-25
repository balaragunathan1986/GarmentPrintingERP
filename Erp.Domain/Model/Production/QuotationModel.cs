using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;

namespace Erp.Domain.Model.Production
{
    public class QuotationModel : AuditFields
    {
        public int Id { get; set; }
        public Guid Quotation_Id { get; set; }
        public string Quotation_Number { get; set; } = string.Empty;
        public int Company_Id { get; set; }
        public int Supplier_Id { get; set; }
        public Guid? Style_Id { get; set; }
        public DateTime? Quotation_Date { get; set; }
        public string? PrintQuality { get; set; }
        public string? Grade { get; set; }
        public decimal? NoofColours { get; set; }
        public bool? Commit { get; set; }
        public bool? Cancel { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public int Validity_Days { get; set; } = 30;
        public DateTime? Valid_Until { get; set; }
        public string Conversion_Status { get; set; } = "NotConverted";
        public Guid? Converted_Invoice_Id { get; set; }
        public DateTime? Converted_Date { get; set; }
        public string? Remarks { get; set; }
        public string? QuotationImagePath { get; set; }
        public decimal? ProfitPercentage { get; set; }
        public decimal? Total { get; set; }
        public decimal? OverHeads { get; set; }
        public decimal? PcsPerRate { get; set; }
        public decimal? OverRate { get; set; }
        public decimal? FinalRatePerPcs { get; set; }
        public List<QuotationDetailDTO> Quotation_Details { get; set; } = new();
        public List<QuotationChargeDTO>? Quotation_Charges { get; set; }
    }
}