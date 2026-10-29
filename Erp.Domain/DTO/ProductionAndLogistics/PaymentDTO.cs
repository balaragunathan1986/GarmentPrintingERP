using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Payment_Master")]
    public class PaymentMasterDTO
    {
        [Key]
        public int PaymentID { get; set; }
        public Guid Payment_Id { get; set; }
        public string PaymentNo { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public Guid Supplier_ID { get; set; }
        [NotMapped]
        public string SupplierName { get; set; } = string.Empty;
        public int? Company_Id { get; set; }
        public Guid? PaymentMode_Id { get; set; }
        public Guid? Account_Id { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime? ReferenceDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string? Remarks { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
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
        public List<PaymentDetailDTO> Details { get; set; } = new();

        [NotMapped]
        public List<PaymentChargeDTO> Charges { get; set; } = new();

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PaymentDetailDTO>? Payment_Details
        {
            get => null;
            set => Details = value ?? new();
        }

        [NotMapped]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PaymentChargeDTO>? Payment_Charges
        {
            get => null;
            set => Charges = value ?? new();
        }
    }

    [Table("Payment_Detail")]
    public class PaymentDetailDTO
    {
        [Key]
        public int PaymentDetailID { get; set; }
        public Guid PaymentDetail_Id { get; set; }
        public Guid Payment_Id { get; set; }
        public Guid Invoice_Id { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PreviousPaidAmount { get; set; }
        public decimal PaidAmount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal BalanceAmount { get; set; }

        public string? Remarks { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        [NotMapped]
        public string InvoiceNo { get; set; } = string.Empty;
        [NotMapped]
        public DateTime? InvoiceDate { get; set; }
    }

    [Table("Payment_Charge")]
    public class PaymentChargeDTO
    {
        [Key]
        public int Id { get; set; }
        public Guid Payment_Charge_Id { get; set; }
        public Guid Charge_Id { get; set; }
        public Guid Payment_Id { get; set; }
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

    public class PaymentInvoiceOptionDTO
    {
        public Guid Invoice_Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime? InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal PreviousPaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public int? Company_Id { get; set; }
    }
}
