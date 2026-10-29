using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO
{
    [Table("Item")]
    public class ItemDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Item_Id { get; set; }
        public string Item_Code { get; set; } = string.Empty;
        public string Item_Name { get; set; } = string.Empty;
        public string Item_Type { get; set; } = string.Empty;
        public Guid? Uom_Id { get; set; }
        public string? Hsn_Code { get; set; }
        public string? Description { get; set; }
        public string? Barcode { get; set; }
        public string? Customer_Item_Code { get; set; }
    }
}