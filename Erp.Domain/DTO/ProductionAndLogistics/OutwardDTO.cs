using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Outward")]
    public class OutwardDTO : AuditFields
    {
        [Key]
        public int OutwardID { get; set; }
        public Guid Outward_Id { get; set; }
        public string OutwardNo { get; set; } = string.Empty;
        public Guid Inward_Id { get; set; }
        public DateTime OutwardDate { get; set; }
        public int? ProcessTypeID { get; set; }
        public int? Company_Id { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
        public string? DeliveryChallanNo { get; set; }
        public string? VehicleNo { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Inward_Number { get; set; }

        [NotMapped]
        public List<OutwardDetailDTO> Details { get; set; } = new();
    }

    [Table("Outward_Detail")]
    public class OutwardDetailDTO : AuditFields
    {
        [Key]
        public int OutwardDetailID { get; set; }
        public Guid OutwardDetail_Id { get; set; }
        public Guid Outward_Id { get; set; }
        public Guid Inward_ID { get; set; }
        public Guid Inward_Item_Id { get; set; }
        public Guid Item_ID { get; set; }
        public Guid Color_ID { get; set; }
        public Guid Size_ID { get; set; }
        public decimal InwardQty { get; set; }
        public decimal OutwardQty { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal BalQty { get; set; }

        public string? Remarks { get; set; }

        [NotMapped]
        public decimal BalanceQty { get; set; }

        [NotMapped]
        public string? Item_Name { get; set; }

        [NotMapped]
        public string? Color_Name { get; set; }

        [NotMapped]
        public string? Size_Name { get; set; }
    }
}