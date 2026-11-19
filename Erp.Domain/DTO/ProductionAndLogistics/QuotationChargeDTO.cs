using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Quotation_Charge")]
    public class QuotationChargeDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Quotation_Charge_Id { get; set; }
        public Guid Quotation_Id { get; set; }
        public Guid Charge_Id { get; set; }
        [Column(TypeName = "char(1)")]
        public string AddLessType { get; set; } = "+";
        public string? RateType { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string ChargeName { get; set; } = string.Empty;
    }
}
