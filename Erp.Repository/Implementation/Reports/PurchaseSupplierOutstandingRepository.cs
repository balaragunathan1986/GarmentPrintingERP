using Erp.Domain.DTO.Reports;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Reports;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Erp.Repository.Implementation.Reports
{
    public class PurchaseSupplierOutstandingRepository : IPurchaseSupplierOutstandingRepository
    {
        private readonly AppDBContext _dbContext;

        public PurchaseSupplierOutstandingRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PurchaseSupplierOutstandingResponseDTO> GetReport(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var searchParameter = new SqlParameter("@SearchText", SqlDbType.NVarChar, 100)
            {
                Value = string.IsNullOrWhiteSpace(searchText) ? DBNull.Value : searchText
            };
            var fromDateParameter = new SqlParameter("@FromDate", SqlDbType.DateTime2)
            {
                Value = fromDate ?? (object)DBNull.Value
            };
            var toDateParameter = new SqlParameter("@ToDate", SqlDbType.DateTime2)
            {
                Value = toDate ?? (object)DBNull.Value
            };

            var rows = await _dbContext
                .Set<PurchaseSupplierOutstandingDTO>()
                .FromSqlRaw(
                    @"EXEC dbo.GetPurchaseSupplierOutstandingReport
                        @SearchText,
                        @FromDate,
                        @ToDate",
                    searchParameter,
                    fromDateParameter,
                    toDateParameter)
                .AsNoTracking()
                .ToListAsync();

            return new PurchaseSupplierOutstandingResponseDTO { Rows = rows };
        }
    }
}