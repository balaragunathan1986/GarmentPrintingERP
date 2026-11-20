using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("MontlyExpenses_Details")]
    public class MontlyExpensesDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid MontlyExpenses_Details_Id { get; set; }
        public Guid Charge_Id { get; set; }
        public Guid MontlyExpenses_Id { get; set; }
        [Column(TypeName = "char(1)")]
        public string AddLessType { get; set; } = "+";
        [Column(TypeName = "numeric(5, 2)")]
        public decimal Percentage { get; set; }
        [Column(TypeName = "numeric(12, 2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "nvarchar(1000)")]
        public string? Remarks { get; set; }

        [NotMapped]
        public string ChargeName { get; set; } = string.Empty;
    }
}
