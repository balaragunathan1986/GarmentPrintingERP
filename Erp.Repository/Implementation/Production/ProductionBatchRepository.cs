using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class ProductionBatchRepository : IProductionBatchRepository
    {
        private readonly AppDBContext _dbContext;

        public ProductionBatchRepository(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task<string> GenerateNumber()
        {
            var JobReceiptNumber = await _dbContext.ProductionBatch
                .OrderByDescending(x => x.Production_Batch_Number)
                .Select(itm => itm.Production_Batch_Number)
                .FirstOrDefaultAsync();
            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.ProductionBatch, Utility.CommonFunctions.GetTransactionNumber(JobReceiptNumber));
        }

        public async Task<List<JobReceiptListDTO>> GetOpenJobReceipts(DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate",   (object?)toDate   ?? DBNull.Value);

            return await _dbContext.JobReceiptList
                .FromSqlRaw("EXEC sp_GetOpenJobReceipts @FromDate, @ToDate", p1, p2)
                .ToListAsync();
        }

        public async Task<List<JobReceiptListDTO>> GetEditJobReceipts(Guid production_Batch_Id,DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@Production_Batch_Id", (object?)production_Batch_Id ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value);
            var p3 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value);

            return await _dbContext.JobReceiptList
                .FromSqlRaw("EXEC sp_GetOpenJobReceiptsEdit @Production_Batch_Id, @FromDate, @ToDate", p1, p2,p3)
                .ToListAsync();
        }

        public async Task<List<ProductionBatchListDTO>> GetProductionBatchList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@SearchText", (object?)searchText ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate",   (object?)fromDate   ?? DBNull.Value);
            var p3 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate",     (object?)toDate     ?? DBNull.Value);

            return await _dbContext.ProductionBatchList
                .FromSqlRaw("EXEC sp_GetProductionBatchList @SearchText, @FromDate, @ToDate", p1, p2, p3)
                .ToListAsync();
        }

        public async Task<List<ProductionBatchListDTO>> GetProductionBatchCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@SearchText", (object?)searchText ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value);
            var p3 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value);

            return await _dbContext.ProductionBatchList
                .FromSqlRaw("EXEC sp_GetProductionBatchCostList @SearchText, @FromDate, @ToDate", p1, p2, p3)
                .ToListAsync();
        }

        public async Task<List<ProductionBatchDTO>> GetProductionBatch()
        {
            return await _dbContext.ProductionBatch.ToListAsync();
        }

        public async Task<ProductionBatchDTO> GetProductionBatch(Guid Production_Batch_Id)
        {
            var productionBatch = await _dbContext.ProductionBatch.FirstOrDefaultAsync(item => item.Production_Batch_Id == Production_Batch_Id);
            return productionBatch ?? new ProductionBatchDTO();
        }

        public async Task<ProductionBatchModel> Save(ProductionBatchModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Production Batch Master
                // ============================
                var validateNumber = await GenerateNumber();
                if (validateNumber != null)
                {
                    model.Production_Batch_Number = validateNumber == model.Production_Batch_Number ? model.Production_Batch_Number : validateNumber;
                }

                model.Company_Id = 1;

                if (model.Production_Batch_Id == Guid.Empty)
                {
                    model.Production_Batch_Id = Guid.NewGuid();
                }

                var productionBatch = new ProductionBatchDTO
                {
                    Production_Batch_Id = model.Production_Batch_Id,
                    Production_Batch_Number = model.Production_Batch_Number,
                    Company_Id = model.Company_Id,
                    Start_Date = model.Start_Date,
                    End_Date = model.End_Date,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy,
                    Production_Status = model.Production_Status,
                    Supervisor_Id = model.Supervisor_Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.ProductionBatch.AddAsync(productionBatch);
                await _dbContext.SaveChangesAsync();


                // ============================
                // Production Batch Details
                // ============================

                if (model.Production_Batch_Details != null &&
                    model.Production_Batch_Details.Any())
                {
                    var details = model.Production_Batch_Details
                        .Select(detail => new ProductionBatchDetailDTO
                        {
                            Production_Batch_Detail_Id = Guid.NewGuid(),

                            // Link detail to Production Batch Master
                            Production_Batch_Id = model.Production_Batch_Id,

                            // Job Receipt GUID
                            Job_Receipt_Master_Id = detail.Job_Receipt_Master_Id,

                            Is_Selected = detail.Is_Selected,

                            Input_Quantity = detail.Input_Quantity,

                            Remarks = detail.Remarks,

                            CreatedBy = model.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    
                    await _dbContext.ProductionBatchDetails
                        .AddRangeAsync(details);

                    await _dbContext.SaveChangesAsync();
                }


                // ============================
                // Commit Transaction
                // ============================

                await transaction.CommitAsync();

                // Return generated database Id
                model.Id = productionBatch.Id;

                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductionBatchModel> Update(ProductionBatchModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Validate Production Batch Id
                // ============================

                if (model.Production_Batch_Id == Guid.Empty)
                {
                    throw new ArgumentException("Production Batch Id is required.");
                }        

                // ============================
                // Production Batch Master
                // ============================

                var productionBatch = await _dbContext.ProductionBatch
                    .FirstOrDefaultAsync(x =>
                        x.Production_Batch_Id == model.Production_Batch_Id &&
                        x.Company_Id == model.Company_Id &&
                        x.DeletedAt == null);

                if (productionBatch == null)
                {
                    throw new KeyNotFoundException(
                        "Production Batch not found.");
                }

                productionBatch.Production_Batch_Number = model.Production_Batch_Number;
                productionBatch.Start_Date = model.Start_Date;
                productionBatch.End_Date = model.End_Date;
                productionBatch.Remarks = model.Remarks;
                productionBatch.UpdatedBy = model.UpdatedBy;
                productionBatch.UpdatedAt = DateTime.UtcNow;
                productionBatch.Company_Id = 1;
                productionBatch.Production_Status = model.Production_Status;
                productionBatch.Supervisor_Id = model.Supervisor_Id;
                // ============================
                // Production Batch Details
                // ============================

                var existingDetails = await _dbContext.ProductionBatchDetails
                    .Where(x =>
                        x.Production_Batch_Id == model.Production_Batch_Id)
                    .ToListAsync();

                if (existingDetails.Any())
                {
                    _dbContext.ProductionBatchDetails.RemoveRange(existingDetails);
                }

                // Add latest details
                if (model.Production_Batch_Details != null &&
                    model.Production_Batch_Details.Any())
                {
                    var details = model.Production_Batch_Details
                        .Select(detail => new ProductionBatchDetailDTO
                        {
                            Production_Batch_Detail_Id = Guid.NewGuid(),
                            Production_Batch_Id = model.Production_Batch_Id,
                            Job_Receipt_Master_Id = detail.Job_Receipt_Master_Id,
                            Is_Selected = detail.Is_Selected,
                            Input_Quantity =  detail.Input_Quantity,
                            Remarks =  detail.Remarks,
                            CreatedBy =  model.UpdatedBy ?? model.CreatedBy,
                            CreatedAt =  DateTime.UtcNow
                        })
                        .ToList();

                    await _dbContext.ProductionBatchDetails
                        .AddRangeAsync(details);
                }

                // ============================
                // Save
                // ============================

                await _dbContext.SaveChangesAsync();

                // ============================
                // Commit Transaction
                // ============================

                await transaction.CommitAsync();

                // Return database Id
                model.Id = productionBatch.Id;

                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> Delete(Guid productionBatchId)
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Find Production Batch
                // ============================

                var productionBatch = await _dbContext.ProductionBatch
                    .FirstOrDefaultAsync(x =>
                        x.Production_Batch_Id == productionBatchId);

                if (productionBatch == null)
                {
                    return false;
                }

                // ============================
                // Delete Production Batch Details
                // ============================

                var details = await _dbContext.ProductionBatchDetails
                    .Where(x =>
                        x.Production_Batch_Id == productionBatchId)
                    .ToListAsync();

                if (details.Any())
                {
                    _dbContext.ProductionBatchDetails.RemoveRange(details);
                }

                // ============================
                // Delete Production Batch Master
                // ============================

                _dbContext.ProductionBatch.Remove(productionBatch);

                // ============================
                // Save
                // ============================

                await _dbContext.SaveChangesAsync();

                // ============================
                // Commit
                // ============================

                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
