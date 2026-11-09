using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Quotation_Master")]
    public class QuotationMasterDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Quotation_Id { get; set; }
        public string Quotation_Number { get; set; } = string.Empty;
        public int Company_Id { get; set; }
        public int Supplier_Id { get; set; }
        public Guid? Style_Id { get; set; }
        public DateTime? Quotation_Date { get; set; }
        public bool? Commit { get; set; }
        public bool? Cancel { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; } = "Pending";
        [JsonPropertyName("validityDays")]
        public int Validity_Days { get; set; } = 30;
        [JsonPropertyName("validUntil")]
        public DateTime? Valid_Until { get; set; }
        [JsonPropertyName("conversionStatus")]
        public string Conversion_Status { get; set; } = "NotConverted";
        [JsonPropertyName("convertedInvoiceId")]
        public Guid? Converted_Invoice_Id { get; set; }
        [JsonPropertyName("convertedDate")]
        public DateTime? Converted_Date { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }

        [NotMapped]
        public string? Company_Name { get; set; }

        [NotMapped]
        public List<QuotationDetailDTO> Details { get; set; } = new();
    }
}