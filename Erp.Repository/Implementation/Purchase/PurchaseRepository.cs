using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Purchase
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDBContext _dbContext;

        public PurchaseRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.PurchaseMaster
                .OrderByDescending(purchase => purchase.Purchase_No)
                .Select(purchase => purchase.Purchase_No)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Purchase,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<PurchaseMasterListDTO>> GetPurchases(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query =
                from purchase in _dbContext.PurchaseMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on purchase.Supplier_Id equals supplier.Supplier_Id
                join company in _dbContext.Company.AsNoTracking()
                    on purchase.Company_Id equals company.Company_Id
                where purchase.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || purchase.Purchase_No.Contains(searchText)
                        || (supplier.Supplier_Name ?? string.Empty).Contains(searchText)
                        || (company.Company_Name ?? string.Empty).Contains(searchText))
                    && (!fromDate.HasValue
                        || (purchase.Purchase_Date.HasValue && purchase.Purchase_Date.Value >= fromDate.Value))
                    && (!toDate.HasValue
                        || (purchase.Purchase_Date.HasValue
                            && purchase.Purchase_Date.Value < toDate.Value.Date.AddDays(1)))
                orderby purchase.Id descending
                select new PurchaseMasterListDTO
                {
                    Id = purchase.Id,
                    Purchase_Id = purchase.Purchase_Id,
                    Purchase_No = purchase.Purchase_No,
                    Purchase_Date = purchase.Purchase_Date,
                    Company_Id = purchase.Company_Id,
                    Company_Name = company.Company_Name,
                    Supplier_Id = purchase.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name,
                    Sub_Total = purchase.Sub_Total,
                    Discount_Amount = purchase.Discount_Amount,
                    Tax_Amount = purchase.Tax_Amount,
                    Other_Charges = purchase.Other_Charges,
                    Net_Amount = purchase.Net_Amount,
                    IsApproved = purchase.IsApproved,
                    IsClosed = purchase.IsClosed,
                    IsCancelled = purchase.IsCancelled
                };

            return await query.ToListAsync();
        }

        public async Task<PurchaseViewModel?> GetPurchase(Guid purchaseId)
        {
            var master = await (
                from purchase in _dbContext.PurchaseMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on purchase.Supplier_Id equals supplier.Supplier_Id
                join company in _dbContext.Company.AsNoTracking()
                    on purchase.Company_Id equals company.Company_Id
                where purchase.Purchase_Id == purchaseId && purchase.DeletedAt == null
                select new PurchaseMasterDTO
                {
                    Id = purchase.Id,
                    Purchase_Id = purchase.Purchase_Id,
                    Purchase_No = purchase.Purchase_No,
                    Purchase_Date = purchase.Purchase_Date,
                    Company_Id = purchase.Company_Id,
                    Supplier_Id = purchase.Supplier_Id,
                    Reference_No = purchase.Reference_No,
                    PaymentTerms_Id = purchase.PaymentTerms_Id,
                    Sub_Total = purchase.Sub_Total,
                    Discount_Amount = purchase.Discount_Amount,
                    Tax_Percent = purchase.Tax_Percent,
                    Tax_Amount = purchase.Tax_Amount,
                    Other_Charges = purchase.Other_Charges,
                    Net_Amount = purchase.Net_Amount,
                    Remarks = purchase.Remarks,
                    IsApproved = purchase.IsApproved,
                    ApprovedBy = purchase.ApprovedBy,
                    ApprovedAt = purchase.ApprovedAt,
                    IsClosed = purchase.IsClosed,
                    IsCancelled = purchase.IsCancelled,
                    CreatedBy = purchase.CreatedBy,
                    CreatedAt = purchase.CreatedAt,
                    UpdatedBy = purchase.UpdatedBy,
                    UpdatedAt = purchase.UpdatedAt,
                    DeletedBy = purchase.DeletedBy,
                    DeletedAt = purchase.DeletedAt,
                    Supplier_Name = supplier.Supplier_Name,
                    Company_Name = company.Company_Name
                }).FirstOrDefaultAsync();

            if (master == null)
            {
                return null;
            }

            var details = await (
                from detail in _dbContext.PurchaseDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking()
                    on detail.Item_Id equals item.Item_Id
                join uomItem in _dbContext.Uom.AsNoTracking()
                    on detail.UOM_Id equals (Guid?)uomItem.Uom_Id into uoms
                from uom in uoms.DefaultIfEmpty()
                join sizeItem in _dbContext.Size.AsNoTracking()
                    on detail.Size_Id equals (Guid?)sizeItem.Size_Id into sizes
                from size in sizes.DefaultIfEmpty()
                join colorItem in _dbContext.Color.AsNoTracking()
                    on detail.Color_Id equals (Guid?)colorItem.Color_Id into colors
                from color in colors.DefaultIfEmpty()
                where detail.Purchase_Id == purchaseId && detail.DeletedAt == null
                select new PurchaseDetailDTO
                {
                    Id = detail.Id,
                    Purchase_Detail_Id = detail.Purchase_Detail_Id,
                    Purchase_Id = detail.Purchase_Id,
                    Item_Id = detail.Item_Id,
                    UOM_Id = detail.UOM_Id,
                    Size_Id = detail.Size_Id,
                    Color_Id = detail.Color_Id,
                    Quantity = detail.Quantity,
                    Received_Qty = detail.Received_Qty,
                    Rate = detail.Rate,
                    Discount_Amount = detail.Discount_Amount,
                    Tax_Percent = detail.Tax_Percent,
                    Tax_Amount = detail.Tax_Amount,
                    Amount = detail.Amount,
                    Required_Date = detail.Required_Date,
                    Item_Remarks = detail.Item_Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    DeletedBy = detail.DeletedBy,
                    DeletedAt = detail.DeletedAt,
                    Item_Name = item.Item_Name,
                    UOM_Name = uom == null ? null : uom.Uom,
                    Size_Name = size == null ? null : size.Size_Name,
                    Color_Name = color == null ? null : color.Color_Name
                }).ToListAsync();

            var charges = await (
                from purchaseCharge in _dbContext.PurchaseCharges.AsNoTracking()
                join charge in _dbContext.Charge.AsNoTracking()
                    on purchaseCharge.Charge_Id equals charge.Charge_Id
                where purchaseCharge.Purchase_Id == purchaseId && purchaseCharge.DeletedAt == null
                select new PurchaseChargeDTO
                {
                    Id = purchaseCharge.Id,
                    Purchase_Charge_Id = purchaseCharge.Purchase_Charge_Id,
                    Purchase_Id = purchaseCharge.Purchase_Id,
                    Charge_Id = purchaseCharge.Charge_Id,
                    AddLessType = purchaseCharge.AddLessType,
                    Percentage = purchaseCharge.Percentage,
                    Amount = purchaseCharge.Amount,
                    Remarks = purchaseCharge.Remarks,
                    CreatedBy = purchaseCharge.CreatedBy,
                    CreatedAt = purchaseCharge.CreatedAt,
                    UpdatedBy = purchaseCharge.UpdatedBy,
                    UpdatedAt = purchaseCharge.UpdatedAt,
                    DeletedBy = purchaseCharge.DeletedBy,
                    DeletedAt = purchaseCharge.DeletedAt,
                    Charge_Name = charge.ChargeName
                }).ToListAsync();

            return new PurchaseViewModel
            {
                Purchase_Master = master,
                Purchase_Details = details,
                Purchase_Charges = charges
            };
        }

        public async Task<PurchaseViewModel> Save(PurchaseViewModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var source = model.Purchase_Master!;
                var purchase = new PurchaseMasterDTO
                {
                    Purchase_Id = Guid.NewGuid(),
                    Purchase_No = source.Purchase_No,
                    Purchase_Date = source.Purchase_Date ?? now,
                    Company_Id = source.Company_Id,
                    Supplier_Id = source.Supplier_Id,
                    Reference_No = source.Reference_No,
                    PaymentTerms_Id = source.PaymentTerms_Id,
                    Sub_Total = source.Sub_Total,
                    Discount_Amount = source.Discount_Amount,
                    Tax_Percent = source.Tax_Percent,
                    Tax_Amount = source.Tax_Amount,
                    Other_Charges = source.Other_Charges,
                    Net_Amount = source.Net_Amount,
                    Remarks = source.Remarks,
                    IsApproved = source.IsApproved,
                    ApprovedBy = source.ApprovedBy,
                    ApprovedAt = source.ApprovedAt,
                    IsClosed = source.IsClosed,
                    IsCancelled = source.IsCancelled,
                    CreatedBy = source.CreatedBy,
                    CreatedAt = now
                };

                _dbContext.PurchaseMaster.Add(purchase);
                await _dbContext.SaveChangesAsync();

                await AddDetails(purchase.Purchase_Id, model.Purchase_Details, purchase.CreatedBy, now);
                await AddCharges(purchase.Purchase_Id, model.Purchase_Charges, purchase.CreatedBy, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetPurchase(purchase.Purchase_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PurchaseViewModel> Update(PurchaseViewModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var source = model.Purchase_Master!;
                var purchase = await _dbContext.PurchaseMaster.FirstOrDefaultAsync(item =>
                    item.Purchase_Id == source.Purchase_Id && item.DeletedAt == null);
                if (purchase == null)
                {
                    throw new KeyNotFoundException("Purchase record not found.");
                }

                var now = DateTime.UtcNow;
                purchase.Purchase_No = source.Purchase_No;
                purchase.Purchase_Date = source.Purchase_Date ?? purchase.Purchase_Date ?? now;
                purchase.Company_Id = source.Company_Id;
                purchase.Supplier_Id = source.Supplier_Id;
                purchase.Reference_No = source.Reference_No;
                purchase.PaymentTerms_Id = source.PaymentTerms_Id;
                purchase.Sub_Total = source.Sub_Total;
                purchase.Discount_Amount = source.Discount_Amount;
                purchase.Tax_Percent = source.Tax_Percent;
                purchase.Tax_Amount = source.Tax_Amount;
                purchase.Other_Charges = source.Other_Charges;
                purchase.Net_Amount = source.Net_Amount;
                purchase.Remarks = source.Remarks;
                purchase.IsApproved = source.IsApproved;
                purchase.ApprovedBy = source.ApprovedBy;
                purchase.ApprovedAt = source.ApprovedAt;
                purchase.IsClosed = source.IsClosed;
                purchase.IsCancelled = source.IsCancelled;
                purchase.UpdatedBy = source.UpdatedBy;
                purchase.UpdatedAt = now;

                var existingDetails = await _dbContext.PurchaseDetails
                    .Where(detail => detail.Purchase_Id == purchase.Purchase_Id && detail.DeletedAt == null)
                    .ToListAsync();
                foreach (var detail in existingDetails)
                {
                    detail.DeletedBy = source.UpdatedBy;
                    detail.DeletedAt = now;
                }

                var existingCharges = await _dbContext.PurchaseCharges
                    .Where(charge => charge.Purchase_Id == purchase.Purchase_Id && charge.DeletedAt == null)
                    .ToListAsync();
                foreach (var charge in existingCharges)
                {
                    charge.DeletedBy = source.UpdatedBy;
                    charge.DeletedAt = now;
                }

                var auditUser = source.UpdatedBy ?? purchase.CreatedBy;
                await AddDetails(purchase.Purchase_Id, model.Purchase_Details, auditUser, now);
                await AddCharges(purchase.Purchase_Id, model.Purchase_Charges, auditUser, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetPurchase(purchase.Purchase_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid purchaseId, string? deletedBy)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var purchase = await _dbContext.PurchaseMaster.FirstOrDefaultAsync(item =>
                    item.Purchase_Id == purchaseId && item.DeletedAt == null);
                if (purchase == null)
                {
                    await transaction.CommitAsync();
                    return false;
                }

                var now = DateTime.UtcNow;
                purchase.DeletedBy = deletedBy;
                purchase.DeletedAt = now;

                var details = await _dbContext.PurchaseDetails
                    .Where(detail => detail.Purchase_Id == purchaseId && detail.DeletedAt == null)
                    .ToListAsync();
                foreach (var detail in details)
                {
                    detail.DeletedBy = deletedBy;
                    detail.DeletedAt = now;
                }

                var charges = await _dbContext.PurchaseCharges
                    .Where(charge => charge.Purchase_Id == purchaseId && charge.DeletedAt == null)
                    .ToListAsync();
                foreach (var charge in charges)
                {
                    charge.DeletedBy = deletedBy;
                    charge.DeletedAt = now;
                }

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

        private async Task AddDetails(
            Guid purchaseId,
            IEnumerable<PurchaseDetailDTO> details,
            string? createdBy,
            DateTime now)
        {
            foreach (var detail in details)
            {
                detail.Id = 0;
                detail.Purchase_Detail_Id = Guid.NewGuid();
                detail.Purchase_Id = purchaseId;
                detail.CreatedBy = createdBy;
                detail.CreatedAt = now;
                detail.UpdatedBy = null;
                detail.UpdatedAt = null;
                detail.DeletedBy = null;
                detail.DeletedAt = null;
            }

            await _dbContext.PurchaseDetails.AddRangeAsync(details);
        }

        private async Task AddCharges(
            Guid purchaseId,
            IEnumerable<PurchaseChargeDTO> charges,
            string? createdBy,
            DateTime now)
        {
            foreach (var charge in charges)
            {
                charge.Id = 0;
                charge.Purchase_Charge_Id = Guid.NewGuid();
                charge.Purchase_Id = purchaseId;
                charge.CreatedBy = createdBy;
                charge.CreatedAt = now;
                charge.UpdatedBy = null;
                charge.UpdatedAt = null;
                charge.DeletedBy = null;
                charge.DeletedAt = null;
            }

            await _dbContext.PurchaseCharges.AddRangeAsync(charges);
        }
    }
}
