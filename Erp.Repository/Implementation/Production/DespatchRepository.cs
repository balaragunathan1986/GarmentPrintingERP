using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class DespatchRepository : IDespatchRepository
    {
        private readonly AppDBContext _dbContext;

        public DespatchRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.Despatch
                .OrderByDescending(x => x.Production_Despatch_Number)
                .Select(x => x.Production_Despatch_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.Despatch, Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<DespatchListDTO>> GetDespatchList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var query =
                from despatch in _dbContext.Despatch.AsNoTracking()
                join productionBatch in _dbContext.ProductionBatch.AsNoTracking()
                    on despatch.Production_Batch_Id equals productionBatch.Production_Batch_Id
                where despatch.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || despatch.Production_Despatch_Number.Contains(searchText)
                        || productionBatch.Production_Batch_Number.Contains(searchText))
                    && (!fromDate.HasValue || despatch.Despatch_Date >= fromDate)
                    && (!toDate.HasValue || despatch.Despatch_Date < toDate.Value.Date.AddDays(1))
                orderby despatch.Id descending
                select new DespatchListDTO
                {
                    Id = despatch.Id,
                    Production_Despatch_Id = despatch.Production_Despatch_Id,
                    Production_Despatch_Number = despatch.Production_Despatch_Number,
                    Despatch_Date = despatch.Despatch_Date,
                    Production_Batch_Id = despatch.Production_Batch_Id,
                    Production_Batch_Number = productionBatch.Production_Batch_Number,
                    Batch_Qty = despatch.Batch_Qty,
                    Despatch_Qty = despatch.Despatch_Qty,
                    Loss_Qty = despatch.Loss_Qty,
                    Loss_Percentage=despatch.Loss_Percentage,
                };

            return await query.ToListAsync();
        }

        public async Task<DespatchDTO> GetDespatch(Guid productionDespatchId)
        {
            return await (
                from despatch in _dbContext.Despatch.AsNoTracking()
                join productionBatch in _dbContext.ProductionBatch.AsNoTracking()
                    on despatch.Production_Batch_Id equals productionBatch.Production_Batch_Id
                where despatch.Production_Despatch_Id == productionDespatchId && despatch.DeletedAt == null
                select new DespatchDTO
                {
                    Id = despatch.Id,
                    Production_Despatch_Id = despatch.Production_Despatch_Id,
                    Production_Despatch_Number = despatch.Production_Despatch_Number,
                    Despatch_Date = despatch.Despatch_Date,
                    Production_Batch_Id = despatch.Production_Batch_Id,
                    Production_Batch_Number = productionBatch.Production_Batch_Number,
                    Company_Id = despatch.Company_Id,
                    Batch_Qty = despatch.Batch_Qty,
                    First_Weight = despatch.First_Weight,
                    Second_Weight = despatch.Second_Weight,
                    Despatch_Qty = despatch.Despatch_Qty,
                    Loss_Qty = despatch.Loss_Qty,
                    Loss_Percentage = despatch.Loss_Percentage,
                    Remarks = despatch.Remarks
                }).FirstOrDefaultAsync() ?? new DespatchDTO();
        }

        public async Task<DespatchModel> Save(DespatchModel model)
        {
            model.Company_Id = 1;
            if (model.Production_Despatch_Id == Guid.Empty)
                model.Production_Despatch_Id = Guid.NewGuid();

            var validateNumber = await GenerateNumber();
            if (validateNumber != null)
            {
                model.Production_Despatch_Number = validateNumber == model.Production_Despatch_Number ? model.Production_Despatch_Number : validateNumber;
            }

            var despatch = new DespatchDTO
            {
                Production_Despatch_Id = model.Production_Despatch_Id,
                Production_Despatch_Number = model.Production_Despatch_Number,
                Despatch_Date = model.Despatch_Date,
                Production_Batch_Id = model.Production_Batch_Id,
                Company_Id = model.Company_Id,
                Batch_Qty = model.Batch_Qty,
                First_Weight = model.First_Weight,
                Second_Weight = model.Second_Weight,
                Despatch_Qty = model.Despatch_Qty,
                Loss_Qty = model.Loss_Qty,
                Loss_Percentage = model.Loss_Percentage,
                Remarks = model.Remarks,
                CreatedBy = model.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Despatch.AddAsync(despatch);
            await _dbContext.SaveChangesAsync();

            model.Id = despatch.Id;
            return model;
        }

        public async Task<DespatchModel> Update(DespatchModel model)
        {
            if (model.Production_Despatch_Id == Guid.Empty)
                throw new ArgumentException("Production Despatch Id is required.");

            var despatch = await _dbContext.Despatch.FirstOrDefaultAsync(x =>
                x.Production_Despatch_Id == model.Production_Despatch_Id && x.DeletedAt == null);

            if (despatch == null)
                throw new KeyNotFoundException("Despatch not found.");

            despatch.Production_Despatch_Number = model.Production_Despatch_Number;
            despatch.Despatch_Date = model.Despatch_Date;
            despatch.Production_Batch_Id = model.Production_Batch_Id;
            despatch.Company_Id = 1;
            despatch.Batch_Qty = model.Batch_Qty;
            despatch.First_Weight = model.First_Weight;
            despatch.Second_Weight = model.Second_Weight;
            despatch.Despatch_Qty = model.Despatch_Qty;
            despatch.Loss_Percentage = model.Loss_Percentage;
            despatch.Loss_Qty = model.Loss_Qty;
            despatch.Remarks = model.Remarks;
            despatch.UpdatedBy = model.UpdatedBy;
            despatch.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            model.Id = despatch.Id;
            return model;
        }

        public async Task<bool> Delete(Guid productionDespatchId)
        {
            var despatch = await _dbContext.Despatch
                .FirstOrDefaultAsync(x => x.Production_Despatch_Id == productionDespatchId);

            if (despatch == null)
                return false;

            _dbContext.Despatch.Remove(despatch);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}