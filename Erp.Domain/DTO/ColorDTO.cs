using Erp.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTO
{
    [Table("Color")]
    public class ColorDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Color_Id { get; set; }
        public  string? Color_Name { get; set; }
        public string? Color_Description { get; set; }
        public string? Color_Code { get; set; }
        public string? Pantone_Code { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

    }
}
