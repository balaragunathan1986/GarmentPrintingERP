using Erp.Domain.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTO.ProductionAndLogistics
{
    [Table("Inward_Master")]
    public class InwardMasterDTO : AuditFields
    {
        public int Id { get; set; }
        public Guid Inward_Id { get; set; }
        public string? Inward_Number { get; set; }
        public DateTime? Inward_Date { get; set; }
        public string? JobOrder_Number { get; set; }
        public Guid Supplier_Id { get; set; }
        public string? Supplier_Dc_Number { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Supplier_Name { get; set; }
    }

    [Table("Inward_Item")]
    public class InwardItemDTO
    { 
        public int Id { get; set; }
        public Guid Inward_Item_Id { get; set; }
        public Guid Inward_Id { get; set; }
        public Guid Item_Id { get; set; }
        public Guid Color_Id { get; set; }
        public Guid Size_Id { get; set; }
        public decimal Quantity { get; set; }
        public string? Remarks { get; set; }
    
    }

    [Table("Inward_Stock")]
    public class InwardStockDTO : AuditFields
    {
        public int Id { get; set; }
        public Guid Inward_Stock_Id { get; set; }
        public Guid Inward_Item_Id { get; set; }
        public Guid Inward_Id { get; set; }
        public Guid Stock_Id { get; set; }
        [Column(TypeName = "numeric(12,3)")]
        public decimal Quantity { get; set; }
        public Guid? Uom_Id { get; set; }
        public string? Remarks { get; set; }

        [NotMapped]
        public string? Stock_Name { get; set; }
        [NotMapped]
        public string? Item_Name { get; set; }
        [NotMapped]
        public string? Color_Name { get; set; }
        [NotMapped]
        public string? Size_Name { get; set; }
        [NotMapped]
        public decimal Qty { get; set; }
        [NotMapped]
        public decimal BalQty { get; set; }
        [NotMapped]
        public string? Uom_Name { get; set; }
    }
}
