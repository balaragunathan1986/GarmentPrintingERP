using Erp.Domain.DTO.ProductionAndLogistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Model.Production
{
    public class InwardViewModel
    {
        public InwardMasterDTO? Inward_master { get; set; }
        public IEnumerable<InwardItemDTO>? Inward_Items { get; set; }
        public IEnumerable<InwardStockDTO>? Inward_Stocks { get; set; }
    }
}
