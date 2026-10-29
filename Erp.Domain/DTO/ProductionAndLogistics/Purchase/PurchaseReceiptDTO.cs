using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics.Purchase
{
    [Table("PurchaseReceipt_Master")]
    public class PurchaseReceiptMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid PurchaseReceipt_Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Receipt_No { get; set; } = string.Empty;
        public DateTime Receipt_Date { get; set; }
        public Guid Company_Id { get; set; }
        public Guid Supplier_Id { get; set; }
        [StringLength(50)]
        public string? Dc_No { get; set; }
        [Column(TypeName = "date")]
        public DateTime? Dc_Date { get; set; }
        [StringLength(50)]
        public string? Supplier_Invoice_No { get; set; }
        [Column(TypeName = "date")]
        public DateTime? Supplier_Invoice_Date { get; set; }
        [StringLength(1000)]
        public string? Remarks { get; set; }
        public bool IsApproved { get; set; }
        public bool IsCancelled { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }
        [NotMapped]
        public string? Company_Name { get; set; }
    }

    [Table("PurchaseReceipt_Detail")]
    public class PurchaseReceiptDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid PurchaseReceipt_Detail_Id { get; set; }
        public Guid PurchaseReceipt_Id { get; set; }
        public Guid Purchase_Detail_Id { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Received_Qty { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Accepted_Qty { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Rejected_Qty { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Shortage_Qty { get; set; }
        [Column(TypeName = "numeric(15,5)")]
        public decimal Rate { get; set; }
        [StringLength(500)]
        public string? Remarks { get; set; }

        [NotMapped]
        public Guid Purchase_Id { get; set; }
        [NotMapped]
        public string Purchase_No { get; set; } = string.Empty;
        [NotMapped]
        public Guid Item_Id { get; set; }
        [NotMapped]
        public string Item_Name { get; set; } = string.Empty;
        [NotMapped]
        public Guid? UOM_Id { get; set; }
        [NotMapped]
        public Guid? Color_Id { get; set; }
        [NotMapped]
        public Guid? Size_Id { get; set; }
        [NotMapped]
        public decimal Ordered_Qty { get; set; }
        [NotMapped]
        public decimal Previously_Received_Qty { get; set; }
    }
}
