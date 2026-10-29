using Erp.Domain.DTO.ProductionAndLogistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Interfaces.Production
{
    public interface IBillingPdfService
    {
        byte[] GeneratePdf(BillingReportDTO billing);
    }
}
