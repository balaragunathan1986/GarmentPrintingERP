using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Issue_Master")]
    public class IssueMasterDTO
    {
        [Key]
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
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }

        [NotMapped]
        public string? Company_Name { get; set; }

        [NotMapped]
        public List<IssueDetailDTO> Details { get; set; } = new();

        [NotMapped]
        public List<IssueStockAllocationDTO> Stock_Allocations { get; set; } = new();
    }
}
