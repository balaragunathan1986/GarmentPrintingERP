using Erp.Domain.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO
{
    [Table("Size")]
    public class SizeDTO : AuditFields
    {
        [Key]
        public int Id { get; set; }
        public Guid Size_Id { get; set; }
        public string? Size_Name { get; set; }
        public string? Size_Description { get; set; }
        public string? Size_Code { get; set; }
        public Guid Size_Group_Id { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
    }
}
