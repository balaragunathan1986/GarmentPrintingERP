using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO
{
    [Table("StockDetail")]
    public class StockDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Stock_Id { get; set; }
        [Required]
        [StringLength(15)]
        public string StockNo { get; set; } = string.Empty;
        public Guid? ItemId { get; set; }
        public Guid? ColorId { get; set; }
        public Guid? SizeId { get; set; }
        [Column(TypeName = "numeric(14,3)")]
        public decimal Qty { get; set; }
        [Column(TypeName = "numeric(14,4)")]
        public decimal? Rate { get; set; }
        [Column(TypeName = "numeric(14,3)")]
        public decimal? AllotedQty { get; set; }
        public Guid? Product_Id { get; set; }
        [Column(TypeName = "numeric(12,3)")]
        public decimal? SQty { get; set; }
        [Column(TypeName = "numeric(14,3)")]
        public decimal? BalQty { get; set; }
        public DateTime? StockDate { get; set; }
        public Guid? Company_Id { get; set; }
        [Column(TypeName = "numeric(14,3)")]
        public decimal? ReturnQty { get; set; }
        public Guid? Uom_Id { get; set; }
        public Guid? ProcessId { get; set; }
        public Guid? SupplierId { get; set; }
        [StringLength(1000)]
        public string? Remarks { get; set; }
        [Column(TypeName = "numeric(14,4)")]
        public decimal MarkupRate { get; set; }
        [Required]
        [StringLength(30)]
        public string ItemCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
