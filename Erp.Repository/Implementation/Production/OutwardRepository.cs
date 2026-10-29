using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class OutwardRepository : IOutwardRepository
    {
        private readonly AppDBContext _dbContext;

        public OutwardRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.Outward
                .OrderByDescending(outward => outward.OutwardNo)
                .Select(outward => outward.OutwardNo)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Outward,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<OutwardDTO>> GetOutwards()
        {
            var outwards = await _dbContext.Outward.AsNoTracking()
                .Where(outward => outward.DeletedAt == null)
                .OrderByDescending(outward => outward.OutwardID)
                .ToListAsync();

            foreach (var outward in outwards)
            {
                outward.Inward_Number = await _dbContext.InwardMaster.AsNoTracking()
                    .Where(inward => inward.Inward_Id == outward.Inward_Id)
                    .Select(inward => inward.Inward_Number)
                    .FirstOrDefaultAsync();
            }

            return outwards;
        }

        public async Task<OutwardDTO> GetOutward(Guid outwardId)
        {
            var outward = await _dbContext.Outward.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Outward_Id == outwardId && item.DeletedAt == null);

            if (outward == null)
            {
                return new OutwardDTO();
            }

            outward.Inward_Number = await _dbContext.InwardMaster.AsNoTracking()
                .Where(inward => inward.Inward_Id == outward.Inward_Id)
                .Select(inward => inward.Inward_Number)
                .FirstOrDefaultAsync();

            outward.Details = await (
                from detail in _dbContext.OutwardDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on detail.Item_ID equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking() on detail.Color_ID equals color.Color_Id
                join size in _dbContext.Size.AsNoTracking() on detail.Size_ID equals size.Size_Id
                where detail.Outward_Id == outwardId && detail.DeletedAt == null
                select new OutwardDetailDTO
                {
                    OutwardDetailID = detail.OutwardDetailID,
                    OutwardDetail_Id = detail.OutwardDetail_Id,
                    Outward_Id = detail.Outward_Id,
                    Inward_ID = detail.Inward_ID,
                    Inward_Item_Id = detail.Inward_Item_Id,
                    Item_ID = detail.Item_ID,
                    Color_ID = detail.Color_ID,
                    Size_ID = detail.Size_ID,
                    InwardQty = detail.InwardQty,
                    OutwardQty = detail.OutwardQty,
                    BalQty = detail.BalQty,
                    BalanceQty = detail.InwardQty - detail.OutwardQty,
                    Remarks = detail.Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    Item_Name = item.Item_Name,
                    Color_Name = color.Color_Name,
                    Size_Name = size.Size_Name
                }).ToListAsync();

            return outward;
        }

        public Task<List<InwardMasterDTO>> GetAvailableInwards()
        {
            return GetInwardsWithAvailableBalance();
        }

        private async Task<List<InwardMasterDTO>> GetInwardsWithAvailableBalance()
        {
            var allocations = await (
                from detail in _dbContext.OutwardDetails.AsNoTracking()
                join outward in _dbContext.Outward.AsNoTracking() on detail.Outward_Id equals outward.Outward_Id
                where detail.DeletedAt == null && outward.DeletedAt == null
                group detail by detail.Inward_Item_Id into inwardAllocations
                select new
                {
                    InwardItemId = inwardAllocations.Key,
                    Quantity = inwardAllocations.Sum(detail => detail.OutwardQty)
                }).ToDictionaryAsync(allocation => allocation.InwardItemId, allocation => allocation.Quantity);

            var inwardItems = await _dbContext.InwardItems.AsNoTracking().ToListAsync();
            var availableInwardIds = inwardItems
                .Where(item => item.Quantity > allocations.GetValueOrDefault(item.Inward_Item_Id))
                .Select(item => item.Inward_Id)
                .Distinct()
                .ToList();

            return await _dbContext.InwardMaster.AsNoTracking()
                .Where(inward => inward.DeletedAt == null && availableInwardIds.Contains(inward.Inward_Id))
                .OrderByDescending(inward => inward.Id)
                .ToListAsync();
        }

        public async Task<List<OutwardDetailDTO>> GetInwardItems(Guid inwardId)
        {
            return await (
                from inwardItem in _dbContext.InwardItems.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on inwardItem.Item_Id equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking() on inwardItem.Color_Id equals color.Color_Id
                join size in _dbContext.Size.AsNoTracking() on inwardItem.Size_Id equals size.Size_Id
                where inwardItem.Inward_Id == inwardId
                select new OutwardDetailDTO
                {
                    Inward_ID = inwardItem.Inward_Id,
                    Inward_Item_Id = inwardItem.Inward_Item_Id,
                    Item_ID = inwardItem.Item_Id,
                    Color_ID = inwardItem.Color_Id,
                    Size_ID = inwardItem.Size_Id,
                    InwardQty = inwardItem.Quantity,
                    OutwardQty = 0,
                    BalanceQty = inwardItem.Quantity - _dbContext.OutwardDetails
                        .Where(detail => detail.Inward_Item_Id == inwardItem.Inward_Item_Id
                            && detail.DeletedAt == null
                            && _dbContext.Outward.Any(outward => outward.Outward_Id == detail.Outward_Id
                                && outward.DeletedAt == null))
                        .Select(detail => (decimal?)detail.OutwardQty)
                        .Sum() ?? 0,
                    Item_Name = item.Item_Name,
                    Color_Name = color.Color_Name,
                    Size_Name = size.Size_Name,
                    Remarks = inwardItem.Remarks
                }).ToListAsync();
        }

        public async Task<OutwardDTO> Save(OutwardModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                if (model.Outward_Id == Guid.Empty)
                {
                    model.Outward_Id = Guid.NewGuid();
                }

                var details = model.Details ?? new List<OutwardDetailDTO>();
                await ValidateAvailableQuantities(model.Inward_Id, details, null);

                var outward = new OutwardDTO
                {
                    Outward_Id = model.Outward_Id,
                    OutwardNo = model.OutwardNo,
                    Inward_Id = model.Inward_Id,
                    OutwardDate = model.OutwardDate,
                    ProcessTypeID = model.ProcessTypeID,
                    Company_Id = model.Company_Id,
                    ExpectedReturnDate = model.ExpectedReturnDate,
                    DeliveryChallanNo = model.DeliveryChallanNo,
                    VehicleNo = model.VehicleNo,
                    Status = model.Status,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy ?? string.Empty,
                    CreatedAt = model.CreatedAt ?? now
                };

                await _dbContext.Outward.AddAsync(outward);
                await _dbContext.SaveChangesAsync();
                await _dbContext.OutwardDetails.AddRangeAsync(details.Select(detail => CreateDetail(
                    detail, outward.Outward_Id, outward.Inward_Id, outward.CreatedBy, now)));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOutward(outward.Outward_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OutwardDTO> Update(OutwardModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var outward = await _dbContext.Outward.FirstOrDefaultAsync(item =>
                    item.Outward_Id == model.Outward_Id && item.DeletedAt == null);
                if (outward == null)
                {
                    throw new KeyNotFoundException("Outward record not found.");
                }

                var details = model.Details ?? new List<OutwardDetailDTO>();
                await ValidateAvailableQuantities(model.Inward_Id, details, outward.Outward_Id);

                var now = DateTime.UtcNow;
                outward.OutwardNo = model.OutwardNo;
                outward.Inward_Id = model.Inward_Id;
                outward.OutwardDate = model.OutwardDate;
                outward.ProcessTypeID = model.ProcessTypeID;
                outward.Company_Id = model.Company_Id;
                outward.ExpectedReturnDate = model.ExpectedReturnDate;
                outward.DeliveryChallanNo = model.DeliveryChallanNo;
                outward.VehicleNo = model.VehicleNo;
                outward.Status = model.Status;
                outward.Remarks = model.Remarks;
                outward.UpdatedBy = model.UpdatedBy;
                outward.UpdatedAt = now;

                var existingDetails = await _dbContext.OutwardDetails
                    .Where(detail => detail.Outward_Id == outward.Outward_Id && detail.DeletedAt == null)
                    .ToListAsync();
                foreach (var detail in existingDetails)
                {
                    detail.DeletedBy = model.UpdatedBy ?? model.CreatedBy;
                    detail.DeletedAt = now;
                }

                await _dbContext.OutwardDetails.AddRangeAsync(details.Select(detail => CreateDetail(
                    detail, outward.Outward_Id, outward.Inward_Id,
                    model.UpdatedBy ?? model.CreatedBy ?? string.Empty, now)));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetOutward(outward.Outward_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid outwardId, string? deletedBy)
        {
            var outward = await _dbContext.Outward
                .FirstOrDefaultAsync(item => item.Outward_Id == outwardId && item.DeletedAt == null);
            if (outward == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;
            outward.DeletedBy = deletedBy;
            outward.DeletedAt = now;

            var details = await _dbContext.OutwardDetails
                .Where(detail => detail.Outward_Id == outwardId && detail.DeletedAt == null)
                .ToListAsync();
            foreach (var detail in details)
            {
                detail.DeletedBy = deletedBy;
                detail.DeletedAt = now;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        private async Task ValidateAvailableQuantities(
            Guid inwardId,
            List<OutwardDetailDTO> details,
            Guid? excludeOutwardId)
        {
            var requested = details
                .GroupBy(detail => detail.Inward_Item_Id)
                .ToDictionary(group => group.Key, group => group.Sum(detail => detail.OutwardQty));
            var itemIds = requested.Keys.ToList();
            var inwardItems = await _dbContext.InwardItems.AsNoTracking()
                .Where(item => item.Inward_Id == inwardId && itemIds.Contains(item.Inward_Item_Id))
                .ToDictionaryAsync(item => item.Inward_Item_Id);

            if (inwardItems.Count != itemIds.Count)
            {
                throw new ArgumentException("One or more outward lines do not belong to the selected inward.");
            }

            var priorOutwards = await (
                from detail in _dbContext.OutwardDetails.AsNoTracking()
                join outward in _dbContext.Outward.AsNoTracking() on detail.Outward_Id equals outward.Outward_Id
                where itemIds.Contains(detail.Inward_Item_Id)
                    && detail.DeletedAt == null
                    && outward.DeletedAt == null
                    && (!excludeOutwardId.HasValue || outward.Outward_Id != excludeOutwardId.Value)
                group detail by detail.Inward_Item_Id into allocations
                select new
                {
                    InwardItemId = allocations.Key,
                    Quantity = allocations.Sum(detail => detail.OutwardQty)
                }).ToDictionaryAsync(allocation => allocation.InwardItemId, allocation => allocation.Quantity);

            foreach (var (itemId, quantity) in requested)
            {
                var alreadyOutward = priorOutwards.GetValueOrDefault(itemId);
                if (quantity < 0 || quantity + alreadyOutward > inwardItems[itemId].Quantity)
                {
                    throw new ArgumentException("Outward quantity cannot exceed the available inward balance.");
                }

                var sourceItem = inwardItems[itemId];
                foreach (var detail in details.Where(detail => detail.Inward_Item_Id == itemId))
                {
                    detail.InwardQty = sourceItem.Quantity;
                    detail.Item_ID = sourceItem.Item_Id;
                    detail.Color_ID = sourceItem.Color_Id;
                    detail.Size_ID = sourceItem.Size_Id;
                }
            }
        }

        private static OutwardDetailDTO CreateDetail(
            OutwardDetailDTO detail,
            Guid outwardId,
            Guid inwardId,
            string createdBy,
            DateTime now)
        {
            return new OutwardDetailDTO
            {
                OutwardDetail_Id = Guid.NewGuid(),
                Outward_Id = outwardId,
                Inward_ID = inwardId,
                Inward_Item_Id = detail.Inward_Item_Id,
                Item_ID = detail.Item_ID,
                Color_ID = detail.Color_ID,
                Size_ID = detail.Size_ID,
                InwardQty = detail.InwardQty,
                OutwardQty = detail.OutwardQty,
                Remarks = detail.Remarks,
                CreatedBy = createdBy,
                CreatedAt = now
            };
        }
    }
}