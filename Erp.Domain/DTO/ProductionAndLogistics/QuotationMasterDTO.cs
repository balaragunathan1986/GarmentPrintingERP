using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Quotation_Master")]
    public class QuotationMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Quotation_Id { get; set; }
        public string Quotation_Number { get; set; } = string.Empty;
        public int Company_Id { get; set; }
        public int Supplier_Id { get; set; }
        public DateTime? Quotation_Date { get; set; }
        public bool? Commit { get; set; }
        public bool? Cancel { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }

        [NotMapped]
        public string? Company_Name { get; set; }

        [NotMapped]
        public List<QuotationDetailDTO> Details { get; set; } = new();
    }
}