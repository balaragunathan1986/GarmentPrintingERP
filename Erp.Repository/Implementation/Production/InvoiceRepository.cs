using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Erp.Repository.Implementation.Production
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDBContext _dbContext;

        public InvoiceRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.InvoiceMaster
                .OrderByDescending(invoice => invoice.InvoiceNo)
                .Select(invoice => invoice.InvoiceNo)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Invoice,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<InvoiceListDTO>> GetInvoices(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var query =
                from invoice in _dbContext.InvoiceMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on invoice.Supplier_ID equals supplier.Supplier_Id
                where invoice.DeletedDate == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || invoice.InvoiceNo.Contains(searchText)
                        || (supplier.Supplier_Name ?? string.Empty).Contains(searchText))
                    && (!fromDate.HasValue || invoice.InvoiceDate >= fromDate)
                    && (!toDate.HasValue || invoice.InvoiceDate < toDate.Value.Date.AddDays(1))
                orderby invoice.InvoiceID descending
                select new InvoiceListDTO
                {
                    InvoiceID = invoice.InvoiceID,
                    Invoice_Id = invoice.Invoice_Id,
                    InvoiceNo = invoice.InvoiceNo,
                    InvoiceDate = invoice.InvoiceDate,
                    Supplier_ID = invoice.Supplier_ID,
                    SupplierName = supplier.Supplier_Name ?? string.Empty,
                    Company_Id = invoice.Company_Id,
                    SubTotal = invoice.SubTotal,
                    AddLessTotal = invoice.AddLessTotal,
                    NetAmount = invoice.NetAmount,
                    ApprovalStatus = invoice.ApprovalStatus
                };

            return await query.ToListAsync();
        }

        public async Task<InvoiceMasterDTO?> GetInvoice(Guid invoiceId)
        {
            var invoice = await (
                from master in _dbContext.InvoiceMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on master.Supplier_ID equals supplier.Supplier_Id
                where master.Invoice_Id == invoiceId && master.DeletedDate == null
                select new InvoiceMasterDTO
                {
                    InvoiceID = master.InvoiceID,
                    Invoice_Id = master.Invoice_Id,
                    InvoiceNo = master.InvoiceNo,
                    InvoiceDate = master.InvoiceDate,
                    Supplier_ID = master.Supplier_ID,
                    SupplierName = supplier.Supplier_Name ?? string.Empty,
                    Company_Id = master.Company_Id,
                    SubTotal = master.SubTotal,
                    AddLessTotal = master.AddLessTotal,
                    NetAmount = master.NetAmount,
                    Remarks = master.Remarks,
                    CreatedBy = master.CreatedBy,
                    CreatedDate = master.CreatedDate,
                    UpdatedBy = master.UpdatedBy,
                    UpdatedDate = master.UpdatedDate,
                    ApprovedBy = master.ApprovedBy,
                    ApprovedDate = master.ApprovedDate,
                    ApprovalStatus = master.ApprovalStatus
                }).FirstOrDefaultAsync();

            if (invoice == null)
            {
                return null;
            }

            invoice.Details = await (
                from detail in _dbContext.InvoiceDetails.AsNoTracking()
                join outward in _dbContext.Outward.AsNoTracking()
                    on detail.Outward_ID equals outward.Outward_Id
                join item in _dbContext.Item.AsNoTracking()
                    on detail.Item_ID equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking()
                    on detail.Color_ID equals color.Color_Id
                join size in _dbContext.Size.AsNoTracking()
                    on detail.Size_ID equals size.Size_Id
                where detail.Invoice_Id == invoiceId && detail.DeletedAt == null
                select new InvoiceDetailDTO
                {
                    InvoiceDetailID = detail.InvoiceDetailID,
                    InvoiceDetail_Id = detail.InvoiceDetail_Id,
                    Invoice_Id = detail.Invoice_Id,
                    Outward_ID = detail.Outward_ID,
                    OutwardDetail_Id = detail.OutwardDetail_Id,
                    OutwardNo = outward.OutwardNo,
                    Item_ID = detail.Item_ID,
                    ItemName = item.Item_Name,
                    Color_ID = detail.Color_ID,
                    ColorName = color.Color_Name ?? string.Empty,
                    Size_ID = detail.Size_ID,
                    SizeName = size.Size_Name ?? string.Empty,
                    InwardQty = detail.InwardQty,
                    OutwardQty = detail.OutwardQty,
                    Rate = detail.Rate,
                    Amount = detail.Amount,
                    Remarks = detail.Remarks
                }).ToListAsync();

            invoice.Charges = await (
                from invoiceCharge in _dbContext.InvoiceCharges.AsNoTracking()
                join charge in _dbContext.Charge.AsNoTracking()
                    on invoiceCharge.Charge_Id equals charge.Charge_Id
                where invoiceCharge.Invoice_Id == invoiceId && invoiceCharge.DeletedAt == null
                select new InvoiceChargeDTO
                {
                    Id = invoiceCharge.Id,
                    Invoice_Charge_Id = invoiceCharge.Invoice_Charge_Id,
                    Charge_Id = invoiceCharge.Charge_Id,
                    ChargeName = charge.ChargeName ?? string.Empty,
                    Invoice_Id = invoiceCharge.Invoice_Id,
                    AddLessType = invoiceCharge.AddLessType,
                    Percentage = invoiceCharge.Percentage,
                    Amount = invoiceCharge.Amount,
                    Remarks = invoiceCharge.Remarks
                }).ToListAsync();

            return invoice;
        }

        public async Task<InvoiceMasterDTO> Save(InvoiceMasterDTO model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;
                var invoiceId = model.Invoice_Id == Guid.Empty ? Guid.NewGuid() : model.Invoice_Id;
                var invoice = new InvoiceMasterDTO
                {
                    Invoice_Id = invoiceId,
                    InvoiceNo = model.InvoiceNo,
                    InvoiceDate = model.InvoiceDate ?? now,
                    Supplier_ID = model.Supplier_ID,
                    Company_Id = model.Company_Id,
                    SubTotal = model.SubTotal,
                    AddLessTotal = model.AddLessTotal,
                    NetAmount = model.NetAmount,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = model.CreatedDate ?? now,
                    ApprovedBy = model.ApprovedBy,
                    ApprovedDate = model.ApprovedDate,
                    ApprovalStatus = model.ApprovalStatus
                };

                _dbContext.InvoiceMaster.Add(invoice);
                await _dbContext.SaveChangesAsync();

                await AddDetails(invoice.Invoice_Id, model.Details ?? new(), model.CreatedBy, now);
                await AddCharges(invoice.Invoice_Id, model.Charges ?? new(), model.CreatedBy, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetInvoice(invoice.Invoice_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<InvoiceMasterDTO> Update(InvoiceMasterDTO model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var invoice = await _dbContext.InvoiceMaster.FirstOrDefaultAsync(item =>
                    item.Invoice_Id == model.Invoice_Id && item.DeletedDate == null);
                if (invoice == null)
                {
                    throw new KeyNotFoundException("Invoice record not found.");
                }

                var now = DateTime.UtcNow;
                invoice.InvoiceNo = model.InvoiceNo;
                invoice.InvoiceDate = model.InvoiceDate ?? invoice.InvoiceDate ?? now;
                invoice.Supplier_ID = model.Supplier_ID;
                invoice.Company_Id = model.Company_Id;
                invoice.SubTotal = model.SubTotal;
                invoice.AddLessTotal = model.AddLessTotal;
                invoice.NetAmount = model.NetAmount;
                invoice.Remarks = model.Remarks;
                invoice.UpdatedBy = model.UpdatedBy;
                invoice.UpdatedDate = now;
                invoice.ApprovedBy = model.ApprovedBy;
                invoice.ApprovedDate = model.ApprovedDate;
                invoice.ApprovalStatus = model.ApprovalStatus;

                await ReplaceDetails(invoice.Invoice_Id, model.Details ?? new(), model.UpdatedBy, now);
                await ReplaceCharges(invoice.Invoice_Id, model.Charges ?? new(), model.UpdatedBy, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetInvoice(invoice.Invoice_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid invoiceId, int? deletedBy)
        {
            await using var transaction =
             await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Find Production Batch
                // ============================

                var qtn = await _dbContext.InvoiceMaster
                    .FirstOrDefaultAsync(x =>
                        x.Invoice_Id == invoiceId);

                if (qtn == null)
                {
                    return false;
                }

                // ============================
                // Delete Invoice Carges
                // ============================

                var detailscharge = await _dbContext.InvoiceCharges
                    .Where(x =>
                        x.Invoice_Id == invoiceId)
                    .ToListAsync();

                if (detailscharge.Any())
                {
                    _dbContext.InvoiceCharges.RemoveRange(detailscharge);
                }

                // ============================
                // Delete Invoice Details
                // ============================

                var details = await _dbContext.InvoiceDetails
                    .Where(x =>
                        x.Invoice_Id == invoiceId)
                    .ToListAsync();

                if (details.Any())
                {
                    _dbContext.InvoiceDetails.RemoveRange(details);
                }

                // ============================
                // Delete Invoice Master
                // ============================

                _dbContext.InvoiceMaster.Remove(qtn);

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

        private async Task AddDetails(Guid invoiceId, List<InvoiceDetailDTO> details, int? createdBy, DateTime now)
        {
            var auditUser = createdBy?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var detail in details)
            {
                detail.InvoiceDetailID = 0;
                detail.InvoiceDetail_Id = detail.InvoiceDetail_Id == Guid.Empty
                    ? Guid.NewGuid()
                    : detail.InvoiceDetail_Id;
                detail.Invoice_Id = invoiceId;
                detail.CreatedBy = string.IsNullOrWhiteSpace(detail.CreatedBy) ? auditUser : detail.CreatedBy;
                detail.CreatedAt ??= now;
                detail.UpdatedBy = null;
                detail.UpdatedAt = null;
                detail.DeletedBy = null;
                detail.DeletedAt = null;
            }

            await _dbContext.InvoiceDetails.AddRangeAsync(details);
        }

        private async Task AddCharges(Guid invoiceId, List<InvoiceChargeDTO> charges, int? createdBy, DateTime now)
        {
            var auditUser = createdBy?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var charge in charges)
            {
                charge.Id = 0;
                charge.Invoice_Charge_Id = charge.Invoice_Charge_Id == Guid.Empty
                    ? Guid.NewGuid()
                    : charge.Invoice_Charge_Id;
                charge.Invoice_Id = invoiceId;
                charge.CreatedBy = string.IsNullOrWhiteSpace(charge.CreatedBy) ? auditUser : charge.CreatedBy;
                charge.CreatedAt ??= now;
                charge.UpdatedBy = null;
                charge.UpdatedAt = null;
                charge.DeletedBy = null;
                charge.DeletedAt = null;
            }

            await _dbContext.InvoiceCharges.AddRangeAsync(charges);
        }

        private async Task ReplaceDetails(Guid invoiceId, List<InvoiceDetailDTO> incoming, int? updatedBy, DateTime now)
        {
            var existing = await _dbContext.InvoiceDetails
                .Where(item => item.Invoice_Id == invoiceId)
                .ToListAsync();
            var incomingIds = incoming
                .Where(item => item.InvoiceDetail_Id != Guid.Empty)
                .Select(item => item.InvoiceDetail_Id)
                .ToHashSet();
            var auditUser = updatedBy?.ToString(CultureInfo.InvariantCulture);

            foreach (var detail in existing)
            {
                if (detail.DeletedAt == null && !incomingIds.Contains(detail.InvoiceDetail_Id))
                {
                    detail.DeletedBy = auditUser;
                    detail.DeletedAt = now;
                }
            }

            foreach (var item in incoming)
            {
                var detail = item.InvoiceDetail_Id == Guid.Empty
                    ? null
                    : existing.FirstOrDefault(existingDetail =>
                        existingDetail.InvoiceDetail_Id == item.InvoiceDetail_Id);
                if (detail == null)
                {
                    item.InvoiceDetailID = 0;
                    item.InvoiceDetail_Id = item.InvoiceDetail_Id == Guid.Empty
                        ? Guid.NewGuid()
                        : item.InvoiceDetail_Id;
                    item.Invoice_Id = invoiceId;
                    item.CreatedBy = string.IsNullOrWhiteSpace(item.CreatedBy)
                        ? auditUser ?? string.Empty
                        : item.CreatedBy;
                    item.CreatedAt ??= now;
                    item.UpdatedBy = null;
                    item.UpdatedAt = null;
                    item.DeletedBy = null;
                    item.DeletedAt = null;
                    _dbContext.InvoiceDetails.Add(item);
                    continue;
                }

                detail.Outward_ID = item.Outward_ID;
                detail.OutwardDetail_Id = item.OutwardDetail_Id;
                detail.Item_ID = item.Item_ID;
                detail.Color_ID = item.Color_ID;
                detail.Size_ID = item.Size_ID;
                detail.InwardQty = item.InwardQty;
                detail.OutwardQty = item.OutwardQty;
                detail.Rate = item.Rate;
                detail.Remarks = item.Remarks;
                detail.UpdatedBy = auditUser;
                detail.UpdatedAt = now;
                detail.DeletedBy = null;
                detail.DeletedAt = null;
            }
        }

        private async Task ReplaceCharges(Guid invoiceId, List<InvoiceChargeDTO> incoming, int? updatedBy, DateTime now)
        {
            var existing = await _dbContext.InvoiceCharges
                .Where(item => item.Invoice_Id == invoiceId)
                .ToListAsync();
            var incomingIds = incoming
                .Where(item => item.Invoice_Charge_Id != Guid.Empty)
                .Select(item => item.Invoice_Charge_Id)
                .ToHashSet();
            var auditUser = updatedBy?.ToString(CultureInfo.InvariantCulture);

            foreach (var charge in existing)
            {
                if (charge.DeletedAt == null && !incomingIds.Contains(charge.Invoice_Charge_Id))
                {
                    charge.DeletedBy = auditUser;
                    charge.DeletedAt = now;
                }
            }

            foreach (var item in incoming)
            {
                var charge = item.Invoice_Charge_Id == Guid.Empty
                    ? null
                    : existing.FirstOrDefault(existingCharge =>
                        existingCharge.Invoice_Charge_Id == item.Invoice_Charge_Id);
                if (charge == null)
                {
                    item.Id = 0;
                    item.Invoice_Charge_Id = item.Invoice_Charge_Id == Guid.Empty
                        ? Guid.NewGuid()
                        : item.Invoice_Charge_Id;
                    item.Invoice_Id = invoiceId;
                    item.CreatedBy = string.IsNullOrWhiteSpace(item.CreatedBy)
                        ? auditUser ?? string.Empty
                        : item.CreatedBy;
                    item.CreatedAt ??= now;
                    item.UpdatedBy = null;
                    item.UpdatedAt = null;
                    item.DeletedBy = null;
                    item.DeletedAt = null;
                    _dbContext.InvoiceCharges.Add(item);
                    continue;
                }

                charge.Charge_Id = item.Charge_Id;
                charge.AddLessType = item.AddLessType;
                charge.Percentage = item.Percentage;
                charge.Amount = item.Amount;
                charge.Remarks = item.Remarks;
                charge.UpdatedBy = auditUser;
                charge.UpdatedAt = now;
                charge.DeletedBy = null;
                charge.DeletedAt = null;
            }
        }
    }
}
