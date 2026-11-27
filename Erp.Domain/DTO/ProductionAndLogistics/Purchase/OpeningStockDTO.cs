using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO.ProductionAndLogistics.Purchase
{
    [Table("OpeningStock")]
    public class OpeningStockDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid OpeningStock_Id { get; set; }
        [Required]
        [StringLength(15)]
        public string OpeningStock_Number { get; set; } = string.Empty;
        public DateTime OpeningStock_Date { get; set; }
        public int Company_Id { get; set; }
        public Guid? Stock_Id { get; set; }
        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class OpeningStockSaveRequestDTO
    {
        [Required]
        [StringLength(15)]
        public string OpeningStock_Number { get; set; } = string.Empty;
        public DateTime OpeningStock_Date { get; set; }
        public int Company_Id { get; set; }
        public Guid Company_GUID { get; set; }
        [StringLength(500)]
        public string? Remarks { get; set; }
        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public List<OpeningStockDetailSaveRequestDTO> OpeningStock_Details { get; set; } = new();
    }

    public class OpeningStockDetailSaveRequestDTO
    {
        public int Id { get; set; }
        public Guid OpeningStock_Id { get; set; }
        public Guid Stock_Id { get; set; }
        public Guid? Item_Id { get; set; }
        public string? Item_Code { get; set; }
        public string? Item_Name { get; set; }
        public string? Item_Type { get; set; }
        public Guid? Color_Id { get; set; }
        public string? Color_Code { get; set; }
        public string? Color_Name { get; set; }
        public Guid? Size_Id { get; set; }
        public string? Size_Code { get; set; }
        public string? Size_Name { get; set; }
        public Guid? Size_Group_Id { get; set; }
        public Guid? Uom_Id { get; set; }
        public string? Uom_Name { get; set; }
        public string? Uom_Abbreviation { get; set; }
        [Required]
        public StockDetailDTO StockDetail { get; set; } = new();
    }

    public class OpeningStockViewDTO
    {
        public int Id { get; set; }
        public string OpeningStock_Number { get; set; } = string.Empty;
        public DateTime OpeningStock_Date { get; set; }
        public int Company_Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OpeningStockLineViewDTO> Details { get; set; } = new();
    }

    public class OpeningStockLineViewDTO
    {
        public int Id { get; set; }
        public Guid OpeningStock_Id { get; set; }
        public Guid Stock_Id { get; set; }
        public string StockNo { get; set; } = string.Empty;
        public Guid Item_Id { get; set; }
        public string Item_Code { get; set; } = string.Empty;
        public string Item_Name { get; set; } = string.Empty;
        public string Item_Type { get; set; } = string.Empty;
        public Guid? Color_Id { get; set; }
        public string Color_Code { get; set; } = string.Empty;
        public string Color_Name { get; set; } = string.Empty;
        public Guid? Size_Id { get; set; }
        public string Size_Code { get; set; } = string.Empty;
        public string Size_Name { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal? Rate { get; set; }
        public Guid? Uom_Id { get; set; }
        public string Uom_Name { get; set; } = string.Empty;
        public string Uom_Abbreviation { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
