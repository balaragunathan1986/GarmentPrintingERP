using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDBContext _dbContext;

        public DashboardRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardOverviewDTO> GetOverview(DateTime? fromDate, DateTime? toDate)
        {
            var startDate = fromDate?.Date;
            var endDate = toDate?.Date.AddDays(1);

            var jobReceipts = _dbContext.JobReceipt.AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Where(x => !startDate.HasValue || x.Job_Receipt_Date >= startDate)
                .Where(x => !endDate.HasValue || x.Job_Receipt_Date < endDate);

            var productionBatches = _dbContext.ProductionBatch.AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Where(x => !startDate.HasValue || x.Start_Date >= startDate)
                .Where(x => !endDate.HasValue || x.Start_Date < endDate);

            var despatches = _dbContext.Despatch.AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Where(x => !startDate.HasValue || x.Despatch_Date >= startDate)
                .Where(x => !endDate.HasValue || x.Despatch_Date < endDate);

            var assignedReceiptIds = _dbContext.ProductionBatchDetails.AsNoTracking()
                .Where(x => x.DeletedAt == null && x.Is_Selected == true)
                .Select(x => x.Job_Receipt_Master_Id);

            var dispatchedBatchIds = _dbContext.Despatch.AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Select(x => x.Production_Batch_Id);

            var billedDespatchIds = _dbContext.BillingDetails.AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .Select(x => x.Production_Despatch_Id);

            var output = await despatches
                .Where(x => x.Despatch_Date.HasValue)
                .GroupBy(x => x.Despatch_Date!.Value.Date)
                .OrderBy(x => x.Key)
                .Select(x => new DashboardOutputDTO
                {
                    Date = x.Key,
                    Quantity = x.Sum(item => item.Despatch_Qty)
                })
                .ToListAsync();

            var openJobReceipts = await jobReceipts.CountAsync(x => !assignedReceiptIds.Contains(x.Job_Receipt_Id));
            //var inProduction = await productionBatches.CountAsync(x => x.End_Date == null);
            var inProduction = await _dbContext.ProductionBatch
                                    .CountAsync(x => !_dbContext.ProductionCost
                                        .Any(c => c.Production_Batch_Id == x.Production_Batch_Id));

            var readyToDespatch = await productionBatches.CountAsync(x => x.End_Date != null && !dispatchedBatchIds.Contains(x.Production_Batch_Id));
            var pendingBilling = await despatches.CountAsync(x => !billedDespatchIds.Contains(x.Production_Despatch_Id));
            var totalBatches = await productionBatches.CountAsync();
            var dispatchedBatches = await productionBatches.CountAsync(x => dispatchedBatchIds.Contains(x.Production_Batch_Id));
            var recentActivities = await GetRecentActivities(startDate, endDate);

            return new DashboardOverviewDTO
            {
                Open_Job_Receipts = openJobReceipts,
                In_Production = inProduction,
                Ready_To_Despatch = readyToDespatch,
                Pending_Billing = pendingBilling,
                Completed_Production_Batches = totalBatches - inProduction,
                Total_Production_Batches = totalBatches,
                Despatched_Production_Batches = dispatchedBatches,
                Production_Readiness_Percentage = totalBatches == 0 ? 0 : Math.Round((decimal)(totalBatches - inProduction) / totalBatches * 100, 2),
                Despatch_Readiness_Percentage = totalBatches == 0 ? 0 : Math.Round((decimal)dispatchedBatches / totalBatches * 100, 2),
                Total_Production_Output = output.Sum(x => x.Quantity),
                Production_Output = output,
                Recent_Activities = recentActivities
            };
        }

        private async Task<List<DashboardActivityDTO>> GetRecentActivities(DateTime? startDate, DateTime? endDate)
        {
            var jobReceiptActivities = await _dbContext.JobReceipt.AsNoTracking()
                .Where(x => x.DeletedAt == null && x.CreatedAt.HasValue)
                .Where(x => !startDate.HasValue || x.CreatedAt >= startDate)
                .Where(x => !endDate.HasValue || x.CreatedAt < endDate)
                .Select(x => new DashboardActivityDTO
                {
                    Type = "JobReceipt",
                    Title = "Job receipt created",
                    Reference = x.Job_Receipt_Number,
                    Occurred_At = x.CreatedAt!.Value
                })
                .ToListAsync();

            var batchActivities = await _dbContext.ProductionBatch.AsNoTracking()
                .Where(x => x.DeletedAt == null && (x.UpdatedAt ?? x.CreatedAt).HasValue)
                .Where(x => !startDate.HasValue || (x.UpdatedAt ?? x.CreatedAt) >= startDate)
                .Where(x => !endDate.HasValue || (x.UpdatedAt ?? x.CreatedAt) < endDate)
                .Select(x => new DashboardActivityDTO
                {
                    Type = "ProductionBatch",
                    Title = x.End_Date == null ? "Production batch in progress" : "Production batch completed",
                    Reference = x.Production_Batch_Number,
                    Occurred_At = (x.UpdatedAt ?? x.CreatedAt)!.Value
                })
                .ToListAsync();

            var despatchActivities = await _dbContext.Despatch.AsNoTracking()
                .Where(x => x.DeletedAt == null && x.CreatedAt.HasValue)
                .Where(x => !startDate.HasValue || x.CreatedAt >= startDate)
                .Where(x => !endDate.HasValue || x.CreatedAt < endDate)
                .Select(x => new DashboardActivityDTO
                {
                    Type = "Despatch",
                    Title = "Despatch created",
                    Reference = x.Production_Despatch_Number,
                    Occurred_At = x.CreatedAt!.Value
                })
                .ToListAsync();

            var billingActivities = await _dbContext.BillingMaster.AsNoTracking()
                .Where(x => x.DeletedAt == null && x.CreatedAt.HasValue)
                .Where(x => !startDate.HasValue || x.CreatedAt >= startDate)
                .Where(x => !endDate.HasValue || x.CreatedAt < endDate)
                .Select(x => new DashboardActivityDTO
                {
                    Type = "Billing",
                    Title = "Billing created",
                    Reference = x.Billing_Number,
                    Occurred_At = x.CreatedAt!.Value
                })
                .ToListAsync();

            return jobReceiptActivities
                .Concat(batchActivities)
                .Concat(despatchActivities)
                .Concat(billingActivities)
                .OrderByDescending(x => x.Occurred_At)
                .Take(10)
                .ToList();
        }
    }
}