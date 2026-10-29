using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Invoice_Master")]
    public class InvoiceMasterDTO
    {
        [Key]
        public int InvoiceID { get; set; }
        public Guid Invoice_Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public Guid Supplier_ID { get; set; }
        [NotMapped]
        public string SupplierName { get; set; } = string.Empty;
        public int? Company_Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal AddLessTotal { get; set; }
        public decimal NetAmount { get; set; }
        public string? Remarks { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";

        [NotMapped]
        public List<InvoiceDetailDTO> Details { get; set; } = new();

        [NotMapped]
        public List<InvoiceChargeDTO> Charges { get; set; } = new();

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<InvoiceDetailDTO>? Invoice_Details
        {
            get => null;
            set => Details = value ?? new();
        }

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<InvoiceChargeDTO>? Invoice_Charges
        {
            get => null;
            set => Charges = value ?? new();
        }
    }

    [Table("Invoice_Detail")]
    public class InvoiceDetailDTO
    {
        [Key]
        public int InvoiceDetailID { get; set; }
        public Guid InvoiceDetail_Id { get; set; }
        public Guid Invoice_Id { get; set; }
        public Guid Outward_ID { get; set; }
        public Guid OutwardDetail_Id { get; set; }
        public Guid Item_ID { get; set; }
        public Guid Color_ID { get; set; }
        public Guid Size_ID { get; set; }
        public decimal InwardQty { get; set; }
        public decimal OutwardQty { get; set; }
        public decimal Rate { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Amount { get; set; }

        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public string OutwardNo { get; set; } = string.Empty;
        [NotMapped]
        public string ItemName { get; set; } = string.Empty;
        [NotMapped]
        public string ColorName { get; set; } = string.Empty;
        [NotMapped]
        public string SizeName { get; set; } = string.Empty;
    }

    [Table("Invoice_Charge")]
    public class InvoiceChargeDTO
    {
        [Key]
        public int Id { get; set; }
        public Guid Invoice_Charge_Id { get; set; }
        public Guid Charge_Id { get; set; }
        public Guid Invoice_Id { get; set; }
        public string AddLessType { get; set; } = "+";
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public string ChargeName { get; set; } = string.Empty;
    }

    public class InvoiceListDTO
    {
        public int InvoiceID { get; set; }
        public Guid Invoice_Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public Guid Supplier_ID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int? Company_Id { get; set; }
        public decimal SubTotal { get; set; }
        public decimal AddLessTotal { get; set; }
        public decimal NetAmount { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
    }
}
