using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Issue_Stock_Allocation")]
    public class IssueStockAllocationDTO
    {
        [Key]
        public long Id { get; set; }
        public Guid Issue_Stock_Allocation_Id { get; set; }
        public Guid Issue_Details_Id { get; set; }
        public Guid Stock_Id { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Allocated_Qty { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
