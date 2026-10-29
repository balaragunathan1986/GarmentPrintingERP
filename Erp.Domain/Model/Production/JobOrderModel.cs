using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;

namespace Erp.Domain.Model.Production
{
    public class JobOrderModel : AuditFields
    {
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
        public List<JobOrderDetailDTO> JobOrder_Details { get; set; } = new();
    }
}
