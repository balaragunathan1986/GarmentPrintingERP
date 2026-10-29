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
        public bool? Commit { get; set; }
        public bool? Cancel { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public string? Remarks { get; set; }
        public List<QuotationDetailDTO> Quotation_Details { get; set; } = new();
    }
}