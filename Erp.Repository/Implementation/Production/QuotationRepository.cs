using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly AppDBContext _dbContext;

        public QuotationRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastQuotationNumber = await _dbContext.QuotationMaster
                .OrderByDescending(quotation => quotation.Quotation_Number)
                .Select(quotation => quotation.Quotation_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Quotation,
                Utility.CommonFunctions.GetTransactionNumber(lastQuotationNumber));
        }

        public async Task<List<QuotationMasterDTO>> GetQuotations()
        {
            return await (
                from quotation in _dbContext.QuotationMaster.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking() on quotation.Company_Id equals company.Id
                join supplier in _dbContext.Supplier.AsNoTracking() on quotation.Supplier_Id equals supplier.Id
                where quotation.DeletedAt == null
                orderby quotation.Id descending
                select new QuotationMasterDTO
                {
                    Id = quotation.Id,
                    Quotation_Id = quotation.Quotation_Id,
                    Quotation_Number = quotation.Quotation_Number,
                    Company_Id = quotation.Company_Id,
                    Supplier_Id = quotation.Supplier_Id,
                    Quotation_Date = quotation.Quotation_Date,
                    Commit = quotation.Commit,
                    Cancel = quotation.Cancel,
                    CreatedBy = quotation.CreatedBy,
                    CreatedAt = quotation.CreatedAt,
                    UpdatedBy = quotation.UpdatedBy,
                    UpdatedAt = quotation.UpdatedAt,
                    DeletedBy = quotation.DeletedBy,
                    DeletedAt = quotation.DeletedAt,
                    ApprovedBy = quotation.ApprovedBy,
                    ApprovedDate = quotation.ApprovedDate,
                    ApprovalStatus = quotation.ApprovalStatus,
                    Remarks = quotation.Remarks,
                    Company_Name = company.Company_Name,
                    Supplier_Name = supplier.Supplier_Name
                }).ToListAsync();
        }

        public async Task<QuotationMasterDTO> GetQuotation(Guid quotationId)
        {
            var quotation = await (
                from master in _dbContext.QuotationMaster.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking() on master.Company_Id equals company.Id
                join supplier in _dbContext.Supplier.AsNoTracking() on master.Supplier_Id equals supplier.Id
                where master.Quotation_Id == quotationId && master.DeletedAt == null
                select new QuotationMasterDTO
                {
                    Id = master.Id,
                    Quotation_Id = master.Quotation_Id,
                    Quotation_Number = master.Quotation_Number,
                    Company_Id = master.Company_Id,
                    Supplier_Id = master.Supplier_Id,
                    Quotation_Date = master.Quotation_Date,
                    Commit = master.Commit,
                    Cancel = master.Cancel,
                    CreatedBy = master.CreatedBy,
                    CreatedAt = master.CreatedAt,
                    UpdatedBy = master.UpdatedBy,
                    UpdatedAt = master.UpdatedAt,
                    DeletedBy = master.DeletedBy,
                    DeletedAt = master.DeletedAt,
                    ApprovedBy = master.ApprovedBy,
                    ApprovedDate = master.ApprovedDate,
                    ApprovalStatus = master.ApprovalStatus,
                    Remarks = master.Remarks,
                    Company_Name = company.Company_Name,
                    Supplier_Name = supplier.Supplier_Name
                }).FirstOrDefaultAsync();

            if (quotation == null)
            {
                return new QuotationMasterDTO();
            }

            quotation.Details = await (
                from detail in _dbContext.QuotationDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on detail.ItemID equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking() on detail.ColorID equals color.Color_Id
                join size in _dbContext.Size.AsNoTracking() on detail.SizeID equals size.Size_Id
                where detail.Quotation_Id == quotationId && detail.DeletedAt == null
                select new QuotationDetailDTO
                {
                    Id = detail.Id,
                    Quotation_Details_Id = detail.Quotation_Details_Id,
                    Quotation_Id = detail.Quotation_Id,
                    ItemID = detail.ItemID,
                    ColorID = detail.ColorID,
                    SizeID = detail.SizeID,
                    UomID = detail.UomID,
                    Qty = detail.Qty,
                    Rate = detail.Rate,
                    AppRate = detail.AppRate,
                    Remarks = detail.Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    DeletedBy = detail.DeletedBy,
                    DeletedAt = detail.DeletedAt,
                    Item_Name = item.Item_Name,
                    Color_Name = color.Color_Name,
                    Size_Name = size.Size_Name
                }).ToListAsync();

            return quotation;
        }

        public async Task<QuotationMasterDTO> Save(QuotationModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                if (model.Quotation_Id == Guid.Empty)
                {
                    model.Quotation_Id = Guid.NewGuid();
                }

                var quotation = new QuotationMasterDTO
                {
                    Quotation_Id = model.Quotation_Id,
                    Quotation_Number = model.Quotation_Number,
                    Company_Id = model.Company_Id,
                    Supplier_Id = model.Supplier_Id,
                    Quotation_Date = model.Quotation_Date,
                    Commit = model.Commit,
                    Cancel = model.Cancel,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = model.CreatedAt ?? now,
                    ApprovedBy = model.ApprovedBy,
                    ApprovedDate = model.ApprovedDate,
                    ApprovalStatus = string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Pending" : model.ApprovalStatus,
                    Remarks = model.Remarks
                };

                await _dbContext.QuotationMaster.AddAsync(quotation);
                await _dbContext.SaveChangesAsync();

                var details = (model.Quotation_Details ?? new List<QuotationDetailDTO>())
                    .Select(detail => CreateDetail(detail, model.Quotation_Id, model.CreatedBy, now))
                    .ToList();

                if (details.Count > 0)
                {
                    await _dbContext.QuotationDetails.AddRangeAsync(details);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetQuotation(model.Quotation_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<QuotationMasterDTO> Update(QuotationModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Quotation_Id == Guid.Empty)
                {
                    throw new ArgumentException("Quotation Id is required.");
                }

                var quotation = await _dbContext.QuotationMaster.FirstOrDefaultAsync(item =>
                    item.Quotation_Id == model.Quotation_Id && item.DeletedAt == null);

                if (quotation == null)
                {
                    throw new KeyNotFoundException("Quotation not found.");
                }

                var now = DateTime.UtcNow;
                quotation.Quotation_Number = model.Quotation_Number;
                quotation.Company_Id = model.Company_Id;
                quotation.Supplier_Id = model.Supplier_Id;
                quotation.Quotation_Date = model.Quotation_Date;
                quotation.Commit = model.Commit;
                quotation.Cancel = model.Cancel;
                quotation.UpdatedBy = model.UpdatedBy;
                quotation.UpdatedAt = now;
                quotation.ApprovedBy = model.ApprovedBy;
                quotation.ApprovedDate = model.ApprovedDate;
                quotation.ApprovalStatus = string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Pending" : model.ApprovalStatus;
                quotation.Remarks = model.Remarks;

                var existingDetails = await _dbContext.QuotationDetails
                    .Where(detail => detail.Quotation_Id == model.Quotation_Id && detail.DeletedAt == null)
                    .ToListAsync();
                var incomingDetails = model.Quotation_Details ?? new List<QuotationDetailDTO>();
                var retainedIds = incomingDetails
                    .Where(detail => detail.Quotation_Details_Id != Guid.Empty)
                    .Select(detail => detail.Quotation_Details_Id)
                    .ToHashSet();
                var updatedBy = model.UpdatedBy ?? model.CreatedBy;

                foreach (var existingDetail in existingDetails.Where(detail => !retainedIds.Contains(detail.Quotation_Details_Id)))
                {
                    existingDetail.DeletedBy = updatedBy;
                    existingDetail.DeletedAt = now;
                }

                foreach (var detail in incomingDetails)
                {
                    var existingDetail = existingDetails.FirstOrDefault(existing =>
                        existing.Quotation_Details_Id == detail.Quotation_Details_Id
                        && detail.Quotation_Details_Id != Guid.Empty);

                    if (existingDetail == null)
                    {
                        await _dbContext.QuotationDetails.AddAsync(CreateDetail(
                            detail,
                            model.Quotation_Id,
                            updatedBy,
                            now));
                        continue;
                    }

                    existingDetail.ItemID = detail.ItemID;
                    existingDetail.ColorID = detail.ColorID;
                    existingDetail.SizeID = detail.SizeID;
                    existingDetail.UomID = detail.UomID;
                    existingDetail.Qty = detail.Qty;
                    existingDetail.Rate = detail.Rate;
                    existingDetail.AppRate = detail.AppRate;
                    existingDetail.Remarks = detail.Remarks;
                    existingDetail.UpdatedBy = updatedBy;
                    existingDetail.UpdatedAt = now;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetQuotation(model.Quotation_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid quotationId, string? deletedBy)
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Find Production Batch
                // ============================

                var qtn = await _dbContext.QuotationMaster
                    .FirstOrDefaultAsync(x =>
                        x.Quotation_Id == quotationId);

                if (qtn == null)
                {
                    return false;
                }

                // ============================
                // Delete Quotation Details
                // ============================

                var details = await _dbContext.QuotationDetails
                    .Where(x =>
                        x.Quotation_Id == quotationId)
                    .ToListAsync();

                if (details.Any())
                {
                    _dbContext.QuotationDetails.RemoveRange(details);
                }

                // ============================
                // Delete Quotation Master
                // ============================

                _dbContext.QuotationMaster.Remove(qtn);

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

        private static QuotationDetailDTO CreateDetail(
            QuotationDetailDTO detail,
            Guid quotationId,
            string? createdBy,
            DateTime createdAt)
        {
            return new QuotationDetailDTO
            {
                Quotation_Details_Id = Guid.NewGuid(),
                Quotation_Id = quotationId,
                ItemID = detail.ItemID,
                ColorID = detail.ColorID,
                SizeID = detail.SizeID,
                UomID = detail.UomID,
                Qty = detail.Qty,
                Rate = detail.Rate,
                AppRate = detail.AppRate,
                Remarks = detail.Remarks,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            };
        }
    }
}