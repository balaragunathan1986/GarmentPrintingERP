using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO
{
    [Table("Style")]
    public class StyleDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid StyleId { get; set; }
        public string Style { get; set; } = string.Empty;
        public Guid Item_Id { get; set; }
        public decimal Rate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
