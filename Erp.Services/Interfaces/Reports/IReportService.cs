using Erp.Domain.DTO.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Interfaces.Reports
{
    public interface IReportService
    {
        Task<List<DespatchSummaryDTO>> GetDespatchSummary(DateTime? fromDate = null, DateTime? toDate = null, int? companyId = null);
    }
}
