using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class JobOrderRepository : IJobOrderRepository
    {
        private readonly AppDBContext _dbContext;

        public JobOrderRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastJobOrderNumber = await _dbContext.JobOrderMaster
                .OrderByDescending(jobOrder => jobOrder.JobOrder_Number)
                .Select(jobOrder => jobOrder.JobOrder_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.JobOrder,
                Utility.CommonFunctions.GetTransactionNumber(lastJobOrderNumber));
        }

        public async Task<List<JobOrderMasterDTO>> GetJobOrders()
        {
            return await BuildMasterQuery()
                .OrderByDescending(jobOrder => jobOrder.Id)
                .ToListAsync();
        }

        public async Task<JobOrderMasterDTO> GetJobOrder(Guid jobOrderId)
        {
            var jobOrder = await BuildMasterQuery()
                .Where(master => master.JobOrder_Id == jobOrderId)
                .FirstOrDefaultAsync();

            if (jobOrder == null)
            {
                return new JobOrderMasterDTO();
            }

            jobOrder.Details = await (
                from detail in _dbContext.JobOrderDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on detail.ItemID equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking() on detail.ColorID equals color.Color_Id
                join size in _dbContext.Size.AsNoTracking() on detail.SizeID equals size.Size_Id
                join uom in _dbContext.Uom.AsNoTracking() on detail.UomID equals uom.Uom_Id into uoms
                from uom in uoms.DefaultIfEmpty()
                where detail.JobOrder_Id == jobOrderId && detail.DeletedAt == null
                select new JobOrderDetailDTO
                {
                    Id = detail.Id,
                    JobOrder_Details_Id = detail.JobOrder_Details_Id,
                    JobOrder_Id = detail.JobOrder_Id,
                    ItemID = detail.ItemID,
                    ColorID = detail.ColorID,
                    SizeID = detail.SizeID,
                    UomID = detail.UomID,
                    Qty = detail.Qty,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    DeletedBy = detail.DeletedBy,
                    DeletedAt = detail.DeletedAt,
                    Item_Name = item.Item_Name,
                    Color_Name = color.Color_Name,
                    Size_Name = size.Size_Name,
                    Uom_Name = uom == null ? null : uom.Uom
                }).ToListAsync();

            return jobOrder;
        }

        public async Task<JobOrderMasterDTO> Save(JobOrderModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                if (model.JobOrder_Id == Guid.Empty)
                {
                    model.JobOrder_Id = Guid.NewGuid();
                }

                var jobOrder = new JobOrderMasterDTO
                {
                    JobOrder_Id = model.JobOrder_Id,
                    JobOrder_Number = model.JobOrder_Number,
                    Job_Order_Ref = model.Job_Order_Ref,
                    Quotation_Id = NormalizeGuid(model.Quotation_Id),
                    Company_Id = model.Company_Id,
                    Supplier_Id = model.Supplier_Id,
                    Style_Id = model.Style_Id,
                    JobOrder_Date = model.JobOrder_Date,
                    Currency = model.Currency,
                    ExcessPer = model.ExcessPer,
                    Rate = model.Rate,
                    JobOrdType = model.JobOrdType,
                    Commit = model.Commit,
                    Cancel = model.Cancel,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = model.CreatedAt ?? now,
                    ApprovedBy = model.ApprovedBy,
                    ApprovedDate = model.ApprovedDate,
                    ApprovalStatus = string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Pending" : model.ApprovalStatus,
                    Remarks = model.Remarks
                };

                await _dbContext.JobOrderMaster.AddAsync(jobOrder);
                await _dbContext.SaveChangesAsync();

                var details = (model.JobOrder_Details ?? new List<JobOrderDetailDTO>())
                    .Select(detail => CreateDetail(detail, model.JobOrder_Id, model.CreatedBy, now))
                    .ToList();

                if (details.Count > 0)
                {
                    await _dbContext.JobOrderDetails.AddRangeAsync(details);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetJobOrder(model.JobOrder_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<JobOrderMasterDTO> Update(JobOrderModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.JobOrder_Id == Guid.Empty)
                {
                    throw new ArgumentException("Job Order Id is required.");
                }

                var jobOrder = await _dbContext.JobOrderMaster.FirstOrDefaultAsync(item =>
                    item.JobOrder_Id == model.JobOrder_Id && item.DeletedAt == null);

                if (jobOrder == null)
                {
                    throw new KeyNotFoundException("Job Order not found.");
                }

                var now = DateTime.UtcNow;
                jobOrder.JobOrder_Number = model.JobOrder_Number;
                jobOrder.Job_Order_Ref = model.Job_Order_Ref;
                jobOrder.Quotation_Id = NormalizeGuid(model.Quotation_Id);
                jobOrder.Company_Id = model.Company_Id;
                jobOrder.Supplier_Id = model.Supplier_Id;
                jobOrder.Style_Id = model.Style_Id;
                jobOrder.JobOrder_Date = model.JobOrder_Date;
                jobOrder.Currency = model.Currency;
                jobOrder.ExcessPer = model.ExcessPer;
                jobOrder.Rate = model.Rate;
                jobOrder.JobOrdType = model.JobOrdType;
                jobOrder.Commit = model.Commit;
                jobOrder.Cancel = model.Cancel;
                jobOrder.UpdatedBy = model.UpdatedBy;
                jobOrder.UpdatedAt = now;
                jobOrder.ApprovedBy = model.ApprovedBy;
                jobOrder.ApprovedDate = model.ApprovedDate;
                jobOrder.ApprovalStatus = string.IsNullOrWhiteSpace(model.ApprovalStatus) ? "Pending" : model.ApprovalStatus;
                jobOrder.Remarks = model.Remarks;

                var existingDetails = await _dbContext.JobOrderDetails
                    .Where(detail => detail.JobOrder_Id == model.JobOrder_Id && detail.DeletedAt == null)
                    .ToListAsync();
                var incomingDetails = model.JobOrder_Details ?? new List<JobOrderDetailDTO>();
                var retainedIds = incomingDetails
                    .Where(detail => detail.JobOrder_Details_Id != Guid.Empty)
                    .Select(detail => detail.JobOrder_Details_Id)
                    .ToHashSet();
                var updatedBy = model.UpdatedBy ?? model.CreatedBy;

                foreach (var existingDetail in existingDetails.Where(detail => !retainedIds.Contains(detail.JobOrder_Details_Id)))
                {
                    existingDetail.DeletedBy = updatedBy;
                    existingDetail.DeletedAt = now;
                }

                foreach (var detail in incomingDetails)
                {
                    var existingDetail = existingDetails.FirstOrDefault(existing =>
                        existing.JobOrder_Details_Id == detail.JobOrder_Details_Id
                        && detail.JobOrder_Details_Id != Guid.Empty);

                    if (existingDetail == null)
                    {
                        await _dbContext.JobOrderDetails.AddAsync(CreateDetail(
                            detail,
                            model.JobOrder_Id,
                            updatedBy,
                            now));
                        continue;
                    }

                    existingDetail.ItemID = detail.ItemID;
                    existingDetail.ColorID = detail.ColorID;
                    existingDetail.SizeID = detail.SizeID;
                    existingDetail.UomID = detail.UomID;
                    existingDetail.Qty = detail.Qty;
                    existingDetail.UpdatedBy = updatedBy;
                    existingDetail.UpdatedAt = now;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return await GetJobOrder(model.JobOrder_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid jobOrderId, string? deletedBy)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var jobOrder = await _dbContext.JobOrderMaster
                    .FirstOrDefaultAsync(x => x.JobOrder_Id == jobOrderId);

                if (jobOrder == null)
                {
                    return false;
                }

                var details = await _dbContext.JobOrderDetails
                    .Where(x => x.JobOrder_Id == jobOrderId)
                    .ToListAsync();

                if (details.Any())
                {
                    _dbContext.JobOrderDetails.RemoveRange(details);
                }

                _dbContext.JobOrderMaster.Remove(jobOrder);

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

        private IQueryable<JobOrderMasterDTO> BuildMasterQuery()
        {
            return
                from master in _dbContext.JobOrderMaster.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking() on master.Company_Id equals company.Id
                join supplier in _dbContext.Supplier.AsNoTracking() on master.Supplier_Id equals supplier.Id
                join quotation in _dbContext.QuotationMaster.AsNoTracking() on master.Quotation_Id equals (Guid?)quotation.Quotation_Id into quotations
                from quotation in quotations.DefaultIfEmpty()
                where master.DeletedAt == null
                select new JobOrderMasterDTO
                {
                    Id = master.Id,
                    JobOrder_Id = master.JobOrder_Id,
                    JobOrder_Number = master.JobOrder_Number,
                    Job_Order_Ref = master.Job_Order_Ref,
                    Quotation_Id = master.Quotation_Id,
                    Company_Id = master.Company_Id,
                    Supplier_Id = master.Supplier_Id,
                    Style_Id = master.Style_Id,
                    JobOrder_Date = master.JobOrder_Date,
                    Currency = master.Currency,
                    ExcessPer = master.ExcessPer,
                    Rate = master.Rate,
                    JobOrdType = master.JobOrdType,
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
                    Supplier_Name = supplier.Supplier_Name,
                    Quotation_Number = quotation == null ? null : quotation.Quotation_Number
                };
        }

        private static Guid? NormalizeGuid(Guid? value)
            => value == Guid.Empty ? null : value;

        private static JobOrderDetailDTO CreateDetail(
            JobOrderDetailDTO detail,
            Guid jobOrderId,
            string? createdBy,
            DateTime createdAt)
        {
            return new JobOrderDetailDTO
            {
                JobOrder_Details_Id = Guid.NewGuid(),
                JobOrder_Id = jobOrderId,
                ItemID = detail.ItemID,
                ColorID = detail.ColorID,
                SizeID = detail.SizeID,
                UomID = detail.UomID,
                Qty = detail.Qty,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            };
        }
    }
}
