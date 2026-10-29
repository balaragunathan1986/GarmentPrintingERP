using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("JobOrder_Master")]
    public class JobOrderMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid JobOrder_Id { get; set; }
        public string JobOrder_Number { get; set; } = string.Empty;
        public string? Job_Order_Ref { get; set; }
        public Guid? Quotation_Id { get; set; }
        public int Company_Id { get; set; }
        public int Supplier_Id { get; set; }
        public DateTime? JobOrder_Date { get; set; }
        public string? Currency { get; set; }
        public decimal? ExcessPer { get; set; }
        public decimal? Rate { get; set; }
        public string? JobOrdType { get; set; }
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
        public string? Quotation_Number { get; set; }

        [NotMapped]
        public List<JobOrderDetailDTO> Details { get; set; } = new();
    }
}
