using Erp.Domain.DTO.Reports;
using Erp.Repository.Interfaces.Reports;
using Erp.Services.Interfaces.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportsRepository _reportsRepository;
        public ReportService(IReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }

        public async Task<List<DespatchSummaryDTO>> GetDespatchSummary(DateTime? fromDate = null, DateTime? toDate = null, int? companyId = null)
        {
            return await _reportsRepository.GetDespatchSummary(fromDate, toDate, companyId);
        }
    }
}
