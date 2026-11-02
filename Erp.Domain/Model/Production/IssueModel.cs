using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Domain.Model.Production
{
    public class IssueModel
    {
        public int Id { get; set; }
        public Guid Issue_Id { get; set; }
        public string Issue_Number { get; set; } = string.Empty;
        public DateTime Issue_Date { get; set; }
        public Guid? Supplier_Id { get; set; }
        public string? Issue_Type { get; set; }
        public string? Joborderno { get; set; }
        public Guid? StyleId { get; set; }
        public string Unit_supplier_self { get; set; } = "S";
        public string? GatePassVehicle { get; set; }
        public string? Remarks { get; set; }
        public string? Terms_Conditions { get; set; }
        public int? Company_Id { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public List<IssueDetailDTO> Issue_Details { get; set; } = new();
        public List<IssueStockAllocationDTO> Issue_Stock_Allocations { get; set; } = new();
    }
}
