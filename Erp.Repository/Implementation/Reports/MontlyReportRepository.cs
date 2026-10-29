using Erp.Domain.DTO.Reports;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Reports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Erp.Repository.Implementation.Reports
{
    public class MontlyReportRepository : IMontlyReportRepository
    {
        private readonly AppDBContext _dbContext;

        public MontlyReportRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<MontlyReportResponseDTO> GetReport(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var searchParameter = new SqlParameter("@SearchText", SqlDbType.NVarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(searchText)
                    ? DBNull.Value
                    : searchText
            };

            var fromDateParameter = new SqlParameter("@FromDate", SqlDbType.DateTime2)
            {
                Value = fromDate.HasValue
                    ? fromDate.Value
                    : DBNull.Value
            };

            var toDateParameter = new SqlParameter("@ToDate", SqlDbType.DateTime2)
            {
                Value = toDate.HasValue
                    ? toDate.Value
                    : DBNull.Value
            };

            var rows = await _dbContext
                .Set<MontlyReportDTO>()
                .FromSqlRaw(
                    @"EXEC dbo.GetMonthlyReport
                        @SearchText,
                        @FromDate,
                        @ToDate",
                    searchParameter,
                    fromDateParameter,
                    toDateParameter)
                .AsNoTracking()
                .ToListAsync();

            return new MontlyReportResponseDTO
            {
                Rows = rows
            };
        }
    }
}