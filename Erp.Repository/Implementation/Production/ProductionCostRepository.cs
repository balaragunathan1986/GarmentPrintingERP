using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class ProductionCostRepository : IProductionCostRepository
    {
        private readonly AppDBContext _dbContext;

        public ProductionCostRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.ProductionCost
                .OrderByDescending(x => x.Production_Cost_Number)
                .Select(x => x.Production_Cost_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.ProductionCost, Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<ProductionCostListDTO>> GetProductionCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@SearchText", (object?)searchText ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value);
            var p3 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value);

            return await _dbContext.ProductionCostList
                .FromSqlRaw("EXEC sp_GetProductionCostList @SearchText, @FromDate, @ToDate", p1, p2, p3)
                .ToListAsync();
        }

        public async Task<List<ProductionCostListDTO>> GetProductionSelectedCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var p1 = new Microsoft.Data.SqlClient.SqlParameter("@SearchText", (object?)searchText ?? DBNull.Value);
            var p2 = new Microsoft.Data.SqlClient.SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value);
            var p3 = new Microsoft.Data.SqlClient.SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value);

            return await _dbContext.ProductionCostList
                .FromSqlRaw("EXEC sp_GetProductionCostSelectedList @SearchText, @FromDate, @ToDate", p1, p2, p3)
                .ToListAsync();
        }

        public async Task<List<ProductionCostDTO>> GetProductionCost()
        {
            return await _dbContext.ProductionCost
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<ProductionCostDTO> GetProductionCost(Guid productionCostId)
        {
            var productionCost = await (
                from pc in _dbContext.ProductionCost
                join pb in _dbContext.ProductionBatch
                    on pc.Production_Batch_Id equals pb.Production_Batch_Id
                where pc.Production_Cost_Id == productionCostId
                select new ProductionCostDTO
                {
                    Id = pc.Id,
                    Production_Cost_Id = pc.Production_Cost_Id,
                    Production_Cost_Number = pc.Production_Cost_Number,

                    Production_Batch_Id = pc.Production_Batch_Id,
                    Production_Batch_Number = pb.Production_Batch_Number,

                    Company_Id = pc.Company_Id,
                    Start_Date = pc.Start_Date,
                    End_Date = pc.End_Date,
                    ScreeningStart_Date = pc.ScreeningStart_Date,
                    ScreeningEnd_Date = pc.ScreeningEnd_Date,
                    Total_Expenses = pc.Total_Expenses,
                    Remarks = pc.Remarks
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (productionCost == null)
                return new ProductionCostDTO();

            productionCost.Production_Cost_Charges =
                await _dbContext.ProductionCostCharges
                    .Where(x => x.Production_Cost_Id == productionCostId)
                    .AsNoTracking()
                    .ToListAsync();

            productionCost.Production_Cost_Screenings =
                await _dbContext.ProductionCostScreening
                    .Where(x => x.Production_Cost_Id == productionCostId)
                    .AsNoTracking()
                    .ToListAsync();

            return productionCost;
        }

        public async Task<ProductionCostModel> Save(ProductionCostModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                model.Company_Id = 1;

                if (model.Production_Cost_Id == Guid.Empty)
                    model.Production_Cost_Id = Guid.NewGuid();

                var validateNumber = await GenerateNumber();
                if (validateNumber != null)
                {
                    model.Production_Cost_Number = validateNumber == model.Production_Cost_Number ? model.Production_Cost_Number : validateNumber;
                }

                var productionCost = new ProductionCostDTO
                {
                    Production_Cost_Id = model.Production_Cost_Id,
                    Production_Cost_Number = model.Production_Cost_Number,
                    Production_Batch_Id = model.Production_Batch_Id,
                    Company_Id = model.Company_Id,
                    Start_Date = model.Start_Date,
                    End_Date = model.End_Date,
                    ScreeningStart_Date = model.ScreeningStart_Date,
                    ScreeningEnd_Date = model.ScreeningEnd_Date,
                    Total_Expenses = model.Total_Expenses,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.ProductionCost.AddAsync(productionCost);
                await _dbContext.SaveChangesAsync();

                if (model.Production_Cost_Charges != null && model.Production_Cost_Charges.Any())
                {
                    var charges = model.Production_Cost_Charges
                        .Select(detail => new ProductionCostChargeDTO
                        {
                            Production_Cost_Charges_Id = Guid.NewGuid(),
                            Production_Cost_Id = model.Production_Cost_Id,
                            Charge_Id = detail.Charge_Id,
                            Percentage = detail.Percentage,
                            Amount = detail.Amount,
                            Remarks = detail.Remarks,
                            CreatedBy = model.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await _dbContext.ProductionCostCharges.AddRangeAsync(charges);
                }

                if (model.Production_Cost_Screenings != null && model.Production_Cost_Screenings.Any())
                {
                    var screenings = model.Production_Cost_Screenings
                        .Select(screen => new ProductionCostScreeningDTO
                        {
                            Production_Cost_Screening_Id = Guid.NewGuid(),
                            Production_Cost_Id = model.Production_Cost_Id,
                            Days = screen.Days,
                            Rate = screen.Rate,
                            Persons = screen.Persons,
                            Amount = screen.Amount,
                            Remarks = screen.Remarks,
                            CreatedBy = model.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await _dbContext.ProductionCostScreening.AddRangeAsync(screenings);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                model.Id = productionCost.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductionCostModel> Update(ProductionCostModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Production_Cost_Id == Guid.Empty)
                    throw new ArgumentException("Production Cost Id is required.");

                var productionCost = await _dbContext.ProductionCost
                    .FirstOrDefaultAsync(x =>
                        x.Production_Cost_Id == model.Production_Cost_Id &&
                        x.DeletedAt == null);

                if (productionCost == null)
                    throw new KeyNotFoundException("Production Cost not found.");

                productionCost.Production_Cost_Number = model.Production_Cost_Number;
                productionCost.Production_Batch_Id = model.Production_Batch_Id;
                productionCost.Start_Date = model.Start_Date;
                productionCost.End_Date = model.End_Date;
                productionCost.ScreeningStart_Date = model.ScreeningStart_Date;
                productionCost.ScreeningEnd_Date = model.ScreeningEnd_Date;
                productionCost.Total_Expenses = model.Total_Expenses;
                productionCost.Remarks = model.Remarks;
                productionCost.UpdatedBy = model.UpdatedBy;
                productionCost.UpdatedAt = DateTime.UtcNow;
                productionCost.Company_Id = 1;

                var existingCharges = await _dbContext.ProductionCostCharges
                    .Where(x => x.Production_Cost_Id == model.Production_Cost_Id)
                    .ToListAsync();

                if (existingCharges.Any())
                    _dbContext.ProductionCostCharges.RemoveRange(existingCharges);

                if (model.Production_Cost_Charges != null && model.Production_Cost_Charges.Any())
                {
                    var charges = model.Production_Cost_Charges
                        .Select(detail => new ProductionCostChargeDTO
                        {
                            Production_Cost_Charges_Id = Guid.NewGuid(),
                            Production_Cost_Id = model.Production_Cost_Id,
                            Charge_Id = detail.Charge_Id,
                            Percentage = detail.Percentage,
                            Amount = detail.Amount,
                            Remarks = detail.Remarks,
                            CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await _dbContext.ProductionCostCharges.AddRangeAsync(charges);
                }

                var existingScreenings = await _dbContext.ProductionCostScreening
                    .Where(x => x.Production_Cost_Id == model.Production_Cost_Id)
                    .ToListAsync();

                if (existingScreenings.Any())
                    _dbContext.ProductionCostScreening.RemoveRange(existingScreenings);

                if (model.Production_Cost_Screenings != null && model.Production_Cost_Screenings.Any())
                {
                    var screenings = model.Production_Cost_Screenings
                        .Select(screen => new ProductionCostScreeningDTO
                        {
                            Production_Cost_Screening_Id = Guid.NewGuid(),
                            Production_Cost_Id = model.Production_Cost_Id,
                            Days = screen.Days,
                            Rate = screen.Rate,
                            Amount = screen.Amount,
                            Remarks = screen.Remarks,
                            Persons = screen.Persons,
                            CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    await _dbContext.ProductionCostScreening.AddRangeAsync(screenings);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                model.Id = productionCost.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid productionCostId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var productionCost = await _dbContext.ProductionCost
                    .FirstOrDefaultAsync(x => x.Production_Cost_Id == productionCostId);

                if (productionCost == null)
                    return false;

                await _dbContext.ProductionCostCharges
                    .Where(x => x.Production_Cost_Id == productionCostId)
                    .ExecuteDeleteAsync();

                await _dbContext.ProductionCostScreening
                    .Where(x => x.Production_Cost_Id == productionCostId)
                    .ExecuteDeleteAsync();

                _dbContext.ProductionCost.Remove(productionCost);
                await _dbContext.SaveChangesAsync();
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
