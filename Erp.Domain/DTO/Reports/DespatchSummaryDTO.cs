using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTO.Reports
{
    public class DespatchSummaryDTO
    {
        public string? DespatchNumber { get; set; }
        public DateTime? DespatchDate { get; set; }
        public decimal? DespatchQty { get; set; }
        public decimal? LossQty { get; set; }

        public string? BillingNumber { get; set; }
        public DateTime? BillingDate { get; set; }
        public decimal? NetBillQty { get; set; }

        public string? BillingReceiptNumber { get; set; }
        public DateTime? BillingReceiptDate { get; set; }
        public decimal? BillingAmount { get; set; }
        public decimal? ReceiptAmount { get; set; }
    }
}
