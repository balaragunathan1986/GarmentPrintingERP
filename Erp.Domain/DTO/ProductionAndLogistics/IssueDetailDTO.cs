using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Issue_Details")]
    public class IssueDetailDTO
    {
        [Key]
        public long Id { get; set; }
        public Guid Issue_Details_Id { get; set; }
        public Guid Issue_Id { get; set; }
        public Guid Item_Id { get; set; }
        public Guid? Size_Id { get; set; }
        public Guid? Color_Id { get; set; }
        public Guid UOM_Id { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string? Item_Name { get; set; }

        [NotMapped]
        public string? Color_Name { get; set; }

        [NotMapped]
        public string? Size_Name { get; set; }
    }
}
