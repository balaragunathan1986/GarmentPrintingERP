using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("MontlyExpenses_Master")]
    public class MontlyExpensesDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid MontlyExpenses_Id { get; set; }
        public string MontlyExpenses_Number { get; set; } = string.Empty;
        public DateTime MontlyExpenses { get; set; }
        public int Company_Id { get; set; }
        public Guid? StyleId { get; set; }
        public Guid? JobOrder_Id { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string? Remarks { get; set; }

        [NotMapped]
        public string CompanyName { get; set; } = string.Empty;
        [NotMapped]
        public string StyleName { get; set; } = string.Empty;
        [NotMapped]
        public string JobOrderNumber { get; set; } = string.Empty;
        [NotMapped]
        public List<MontlyExpensesDetailDTO> Details { get; set; } = new();
    }
}
