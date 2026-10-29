using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("JobOrder_Details")]
    public class JobOrderDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid JobOrder_Details_Id { get; set; }
        public Guid JobOrder_Id { get; set; }
        public Guid ItemID { get; set; }
        public Guid ColorID { get; set; }
        public Guid SizeID { get; set; }
        public Guid UomID { get; set; }
        public decimal? Qty { get; set; }

        [NotMapped]
        public string? Item_Name { get; set; }

        [NotMapped]
        public string? Color_Name { get; set; }

        [NotMapped]
        public string? Size_Name { get; set; }

        [NotMapped]
        public string? Uom_Name { get; set; }
    }
}
