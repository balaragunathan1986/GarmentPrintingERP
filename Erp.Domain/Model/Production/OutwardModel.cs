using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;

namespace Erp.Domain.Model.Production
{
    public class OutwardModel : AuditFields
    {
        public int OutwardID { get; set; }
        public Guid Outward_Id { get; set; }
        public string OutwardNo { get; set; } = string.Empty;
        public Guid Inward_Id { get; set; }
        public DateTime OutwardDate { get; set; }
        public int? ProcessTypeID { get; set; }
        public int? Company_Id { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? DeliveryChallanNo { get; set; }
        public string? VehicleNo { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public List<OutwardDetailDTO> Details { get; set; } = new();
    }
}