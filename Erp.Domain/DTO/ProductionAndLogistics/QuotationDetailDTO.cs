using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Quotation_Details")]
    public class QuotationDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Quotation_Details_Id { get; set; }
        public Guid Quotation_Id { get; set; }
        public Guid ItemID { get; set; }
        public Guid ColorID { get; set; }
        public Guid SizeID { get; set; }
        public Guid UomID { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? AppRate { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Item_Name { get; set; }

        [NotMapped]
        public string? Color_Name { get; set; }

        [NotMapped]
        public string? Size_Name { get; set; }
    }
}