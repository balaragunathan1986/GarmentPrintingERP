using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Erp.Domain.DTO
{
    [Table("GeneralSettings")]
    public class GeneralSettingsDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public bool EnableCuttingPercentage { get; set; }
        public bool? ShowQuotationRemarks { get; set; }
    }
}
