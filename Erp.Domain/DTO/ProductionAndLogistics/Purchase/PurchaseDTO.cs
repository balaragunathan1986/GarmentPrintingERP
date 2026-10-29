using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics.Purchase
{
    [Table("Purchase_Master")]
    public class PurchaseMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Purchase_Id { get; set; }
        [Required]
        [StringLength(30)]
        public string Purchase_No { get; set; } = string.Empty;
        public DateTime? Purchase_Date { get; set; }
        public Guid Company_Id { get; set; }
        public Guid Supplier_Id { get; set; }
        [StringLength(50)]
        public string? Reference_No { get; set; }
        public Guid? PaymentTerms_Id { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Sub_Total { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Discount_Amount { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal Tax_Percent { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Tax_Amount { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Other_Charges { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Net_Amount { get; set; }
        [StringLength(1000)]
        public string? Remarks { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCancelled { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }
        [NotMapped]
        public string? Company_Name { get; set; }
    }

    [Table("Purchase_Detail")]
    public class PurchaseDetailDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Purchase_Detail_Id { get; set; }
        public Guid Purchase_Id { get; set; }
        public Guid Item_Id { get; set; }
        public Guid? UOM_Id { get; set; }
        public Guid? Size_Id { get; set; }
        public Guid? Color_Id { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Quantity { get; set; }
        [Column(TypeName = "numeric(15,3)")]
        public decimal Received_Qty { get; set; }
        [Column(TypeName = "numeric(15,5)")]
        public decimal Rate { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Discount_Amount { get; set; }
        [Column(TypeName = "numeric(5,2)")]
        public decimal Tax_Percent { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Tax_Amount { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Amount { get; set; }
        public DateTime? Required_Date { get; set; }
        [StringLength(500)]
        public string? Item_Remarks { get; set; }

        [NotMapped]
        public string? Item_Name { get; set; }
        [NotMapped]
        public string? UOM_Name { get; set; }
        [NotMapped]
        public string? Size_Name { get; set; }
        [NotMapped]
        public string? Color_Name { get; set; }
    }

    [Table("Purchase_Charge")]
    public class PurchaseChargeDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Purchase_Charge_Id { get; set; }
        public Guid Purchase_Id { get; set; }
        public Guid Charge_Id { get; set; }
        [Column(TypeName = "char(1)")]
        public string AddLessType { get; set; } = "+";
        [Column(TypeName = "numeric(5,2)")]
        public decimal Percentage { get; set; }
        [Column(TypeName = "numeric(15,2)")]
        public decimal Amount { get; set; }
        [StringLength(1000)]
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Charge_Name { get; set; }
    }

    public class PurchaseMasterListDTO
    {
        public int Id { get; set; }
        public Guid Purchase_Id { get; set; }
        public string Purchase_No { get; set; } = string.Empty;
        public DateTime? Purchase_Date { get; set; }
        public Guid Company_Id { get; set; }
        public string? Company_Name { get; set; }
        public Guid Supplier_Id { get; set; }
        public string? Supplier_Name { get; set; }
        public decimal Sub_Total { get; set; }
        public decimal Discount_Amount { get; set; }
        public decimal Tax_Amount { get; set; }
        public decimal Other_Charges { get; set; }
        public decimal Net_Amount { get; set; }
        public bool IsApproved { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCancelled { get; set; }
    }
}
