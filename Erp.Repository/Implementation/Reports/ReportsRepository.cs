using Erp.Domain.DTO.Reports;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Reports;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Implementation.Reports
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly AppDBContext _dbcontext;

        public ReportsRepository(AppDBContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<List<DespatchSummaryDTO>> GetDespatchSummary(DateTime? fromDate = null, DateTime? toDate = null, int? companyId = null)
        {
            var despatchQuery = _dbcontext.Despatch
    .Where(d => d.DeletedAt == null)
    .AsQueryable();

            if (fromDate.HasValue)
            {
                despatchQuery = despatchQuery
                    .Where(d => d.Despatch_Date >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                despatchQuery = despatchQuery
                    .Where(d => d.Despatch_Date <= toDate.Value);
            }

            if (companyId.HasValue)
            {
                despatchQuery = despatchQuery
                    .Where(d => d.Company_Id == companyId.Value);
            }

            var result = await (
                from d in despatchQuery

                join bd in _dbcontext.BillingDetails
                    on d.Production_Despatch_Id equals bd.Production_Despatch_Id
                    into bdGroup

                from bd in bdGroup
                    .Where(x => x.DeletedAt == null)
                    .DefaultIfEmpty()

                join bm in _dbcontext.BillingMaster
                    on bd.Billing_Id equals bm.Billing_Id
                    into bmGroup

                from bm in bmGroup
                    .Where(x => x.DeletedAt == null)
                    .DefaultIfEmpty()

                join brm in _dbcontext.BillingReceiptMaster
                    on bm.Billing_Id equals brm.Billing_Id
                    into brmGroup

                from brm in brmGroup
                    .Where(x => x.DeletedAt == null)
                    .DefaultIfEmpty()

                select new DespatchSummaryDTO
                {
                    DespatchNumber = d.Production_Despatch_Number,
                    DespatchDate = d.Despatch_Date,
                    DespatchQty = d.Despatch_Qty,
                    LossQty = d.Loss_Qty,

                    BillingNumber = bm != null
                        ? bm.Billing_Number
                        : null,

                    BillingDate = bm != null
                        ? bm.Billing_Date
                        : null,

                    NetBillQty = bd != null
                        ? bd.Net_Bill_Qty
                        : null,

                    BillingReceiptNumber = brm != null
                        ? brm.Billing_Receipt_Number
                        : null,

                    BillingReceiptDate = brm != null
                        ? brm.Billing_Receipt_Date
                        : null,

                    BillingAmount = brm != null
                        ? brm.Billing_Amount
                        : null,

                    ReceiptAmount = brm != null
                        ? brm.Receipt_Amount
                        : null
                }
            ).ToListAsync();

            return result;

        }
    }
}
