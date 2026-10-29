using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace Erp.Repository.Implementation.Production
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDBContext _dbContext;

        public PaymentRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.PaymentMaster
                .OrderByDescending(payment => payment.PaymentNo)
                .Select(payment => payment.PaymentNo)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Payment,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<PaymentMasterDTO>> GetPayments()
        {
            var payments = await (
                from payment in _dbContext.PaymentMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on payment.Supplier_ID equals supplier.Supplier_Id
                where payment.DeletedDate == null
                orderby payment.PaymentID descending
                select new PaymentMasterDTO
                {
                    PaymentID = payment.PaymentID,
                    Payment_Id = payment.Payment_Id,
                    PaymentNo = payment.PaymentNo,
                    PaymentDate = payment.PaymentDate,
                    Supplier_ID = payment.Supplier_ID,
                    SupplierName = supplier.Supplier_Name ?? string.Empty,
                    Company_Id = payment.Company_Id,
                    PaymentMode_Id = payment.PaymentMode_Id,
                    Account_Id = payment.Account_Id,
                    ReferenceNo = payment.ReferenceNo,
                    ReferenceDate = payment.ReferenceDate,
                    PaymentAmount = payment.PaymentAmount,
                    DeductionAmount = payment.DeductionAmount,
                    NetAmount = payment.NetAmount,
                    Remarks = payment.Remarks,
                    PaymentStatus = payment.PaymentStatus,
                    ApprovalStatus = payment.ApprovalStatus,
                    CreatedBy = payment.CreatedBy,
                    CreatedDate = payment.CreatedDate,
                    UpdatedBy = payment.UpdatedBy,
                    UpdatedDate = payment.UpdatedDate,
                    ApprovedBy = payment.ApprovedBy,
                    ApprovedDate = payment.ApprovedDate
                }).ToListAsync();

            var paymentIds = payments.Select(payment => payment.Payment_Id).ToList();
            if (paymentIds.Count == 0)
            {
                return payments;
            }

            var details = await GetDetails(paymentIds);
            var detailsByPaymentId = details.GroupBy(detail => detail.Payment_Id)
                .ToDictionary(group => group.Key, group => group.ToList());
            var charges = await GetCharges(paymentIds);
            var chargesByPaymentId = charges.GroupBy(charge => charge.Payment_Id)
                .ToDictionary(group => group.Key, group => group.ToList());
            foreach (var payment in payments)
            {
                if (detailsByPaymentId.TryGetValue(payment.Payment_Id, out var paymentDetails))
                {
                    payment.Details = paymentDetails;
                }

                if (chargesByPaymentId.TryGetValue(payment.Payment_Id, out var paymentCharges))
                {
                    payment.Charges = paymentCharges;
                }
            }

            return payments;
        }

        public async Task<PaymentMasterDTO?> GetPayment(Guid paymentId)
        {
            var payment = await (
                from master in _dbContext.PaymentMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on master.Supplier_ID equals supplier.Supplier_Id
                where master.Payment_Id == paymentId && master.DeletedDate == null
                select new PaymentMasterDTO
                {
                    PaymentID = master.PaymentID,
                    Payment_Id = master.Payment_Id,
                    PaymentNo = master.PaymentNo,
                    PaymentDate = master.PaymentDate,
                    Supplier_ID = master.Supplier_ID,
                    SupplierName = supplier.Supplier_Name ?? string.Empty,
                    Company_Id = master.Company_Id,
                    PaymentMode_Id = master.PaymentMode_Id,
                    Account_Id = master.Account_Id,
                    ReferenceNo = master.ReferenceNo,
                    ReferenceDate = master.ReferenceDate,
                    PaymentAmount = master.PaymentAmount,
                    DeductionAmount = master.DeductionAmount,
                    NetAmount = master.NetAmount,
                    Remarks = master.Remarks,
                    PaymentStatus = master.PaymentStatus,
                    ApprovalStatus = master.ApprovalStatus,
                    CreatedBy = master.CreatedBy,
                    CreatedDate = master.CreatedDate,
                    UpdatedBy = master.UpdatedBy,
                    UpdatedDate = master.UpdatedDate,
                    DeletedBy = master.DeletedBy,
                    DeletedDate = master.DeletedDate,
                    ApprovedBy = master.ApprovedBy,
                    ApprovedDate = master.ApprovedDate
                }).FirstOrDefaultAsync();

            if (payment != null)
            {
                payment.Details = await GetDetails(new[] { payment.Payment_Id });
                payment.Charges = await GetCharges(new[] { payment.Payment_Id });
            }

            return payment;
        }

        public async Task<List<PaymentInvoiceOptionDTO>> GetAvailableInvoices(Guid supplierId, Guid? paymentId)
        {
            var invoices = await (
                from invoice in _dbContext.InvoiceMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on invoice.Supplier_ID equals supplier.Supplier_Id
                where invoice.Supplier_ID == supplierId
                    && invoice.DeletedDate == null
                    && supplier.IsDeleted == false
                orderby invoice.InvoiceDate descending, invoice.InvoiceNo
                select new
                {
                    Invoice_Id = invoice.Invoice_Id,
                    InvoiceNo = invoice.InvoiceNo,
                    InvoiceDate = invoice.InvoiceDate,
                    InvoiceAmount = invoice.NetAmount,
                    Company_Id = invoice.Company_Id
                }).ToListAsync();

            if (invoices.Count == 0)
            {
                return new List<PaymentInvoiceOptionDTO>();
            }

            var invoiceIds = invoices.Select(invoice => invoice.Invoice_Id).ToList();
            var paidTotals = await GetActivePaymentDetails(paymentId)
                .Where(detail => invoiceIds.Contains(detail.Invoice_Id))
                .GroupBy(detail => detail.Invoice_Id)
                .Select(group => new
                {
                    Invoice_Id = group.Key,
                    PreviousPaidAmount = group.Sum(detail => detail.PaidAmount)
                })
                .ToListAsync();
            var paidByInvoice = paidTotals.ToDictionary(total => total.Invoice_Id, total => total.PreviousPaidAmount);

            return invoices
                .Select(invoice =>
                {
                    var previousPaidAmount = paidByInvoice.GetValueOrDefault(invoice.Invoice_Id);
                    return new PaymentInvoiceOptionDTO
                    {
                        Invoice_Id = invoice.Invoice_Id,
                        InvoiceNo = invoice.InvoiceNo,
                        InvoiceDate = invoice.InvoiceDate,
                        InvoiceAmount = invoice.InvoiceAmount,
                        PreviousPaidAmount = previousPaidAmount,
                        BalanceAmount = invoice.InvoiceAmount - previousPaidAmount,
                        Company_Id = invoice.Company_Id
                    };
                })
                .Where(invoice => invoice.BalanceAmount > 0)
                .ToList();
        }

        public async Task<PaymentMasterDTO> Save(PaymentMasterDTO model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var now = DateTime.UtcNow;
                var payment = new PaymentMasterDTO
                {
                    Payment_Id = model.Payment_Id == Guid.Empty ? Guid.NewGuid() : model.Payment_Id,
                    PaymentNo = model.PaymentNo,
                    PaymentDate = model.PaymentDate ?? now,
                    Supplier_ID = model.Supplier_ID,
                    Company_Id = model.Company_Id,
                    PaymentMode_Id = model.PaymentMode_Id,
                    Account_Id = model.Account_Id,
                    ReferenceNo = model.ReferenceNo,
                    ReferenceDate = model.ReferenceDate,
                    DeductionAmount = model.DeductionAmount,
                    Remarks = model.Remarks,
                    PaymentStatus = model.PaymentStatus,
                    ApprovalStatus = model.ApprovalStatus,
                    CreatedBy = model.CreatedBy,
                    CreatedDate = model.CreatedDate ?? now,
                    ApprovedBy = model.ApprovedBy,
                    ApprovedDate = model.ApprovedDate
                };

                await SetPaymentAmounts(payment, model.Details, null);
                _dbContext.PaymentMaster.Add(payment);
                await _dbContext.SaveChangesAsync();
                await AddDetails(payment.Payment_Id, model.Details, model.CreatedBy, now);
                await AddCharges(payment.Payment_Id, model.Charges, model.CreatedBy, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetPayment(payment.Payment_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentMasterDTO> Update(PaymentMasterDTO model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var payment = await _dbContext.PaymentMaster.FirstOrDefaultAsync(item =>
                    item.Payment_Id == model.Payment_Id && item.DeletedDate == null);
                if (payment == null)
                {
                    throw new KeyNotFoundException("Payment record not found.");
                }

                var now = DateTime.UtcNow;
                payment.Supplier_ID = model.Supplier_ID;
                payment.DeductionAmount = model.DeductionAmount;
                await SetPaymentAmounts(payment, model.Details, payment.Payment_Id);
                payment.PaymentNo = model.PaymentNo;
                payment.PaymentDate = model.PaymentDate ?? payment.PaymentDate ?? now;
                payment.Supplier_ID = model.Supplier_ID;
                payment.Company_Id = model.Company_Id;
                payment.PaymentMode_Id = model.PaymentMode_Id;
                payment.Account_Id = model.Account_Id;
                payment.ReferenceNo = model.ReferenceNo;
                payment.ReferenceDate = model.ReferenceDate;
                payment.Remarks = model.Remarks;
                payment.PaymentStatus = model.PaymentStatus;
                payment.ApprovalStatus = model.ApprovalStatus;
                payment.UpdatedBy = model.UpdatedBy;
                payment.UpdatedDate = now;
                payment.ApprovedBy = model.ApprovedBy;
                payment.ApprovedDate = model.ApprovedDate;

                await ReplaceDetails(payment.Payment_Id, model.Details, model.UpdatedBy, now);
                await ReplaceCharges(payment.Payment_Id, model.Charges, model.UpdatedBy, now);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetPayment(payment.Payment_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid paymentId, int? deletedBy)
        {
            await using var transaction =
         await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // ============================
                // Find Payment Master
                // ============================

                var qtn = await _dbContext.PaymentMaster
                    .FirstOrDefaultAsync(x =>
                        x.Payment_Id == paymentId);

                if (qtn == null)
                {
                    return false;
                }

                // ============================
                // Delete Payment Details
                // ============================

                var details = await _dbContext.PaymentDetails
                    .Where(x =>
                        x.Payment_Id == paymentId)
                    .ToListAsync();

                if (details.Any())
                {
                    _dbContext.PaymentDetails.RemoveRange(details);
                }

                var charges = await _dbContext.PaymentCharges
                    .Where(charge => charge.Payment_Id == paymentId)
                    .ToListAsync();
                if (charges.Any())
                {
                    _dbContext.PaymentCharges.RemoveRange(charges);
                }

                //// ============================
                //// Delete Invoice Details
                //// ============================

                //var details = await _dbContext.InvoiceDetails
                //    .Where(x =>
                //        x.Invoice_Id == invoiceId)
                //    .ToListAsync();

                //if (details.Any())
                //{
                //    _dbContext.InvoiceDetails.RemoveRange(details);
                //}

                // ============================
                // Delete Invoice Master
                // ============================

                _dbContext.PaymentMaster.Remove(qtn);

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

        private IQueryable<PaymentDetailDTO> GetActivePaymentDetails(Guid? excludedPaymentId)
        {
            return
                from detail in _dbContext.PaymentDetails.AsNoTracking()
                join payment in _dbContext.PaymentMaster.AsNoTracking()
                    on detail.Payment_Id equals payment.Payment_Id
                where detail.DeletedAt == null
                    && payment.DeletedDate == null
                    && payment.PaymentStatus != "Cancelled"
                    && payment.ApprovalStatus != "Rejected"
                    && payment.ApprovalStatus != "Cancelled"
                    && (!excludedPaymentId.HasValue || payment.Payment_Id != excludedPaymentId.Value)
                select detail;
        }

        private async Task<List<PaymentDetailDTO>> GetDetails(IEnumerable<Guid> paymentIds)
        {
            var ids = paymentIds.ToList();
            return await (
                from detail in _dbContext.PaymentDetails.AsNoTracking()
                join invoice in _dbContext.InvoiceMaster.AsNoTracking()
                    on detail.Invoice_Id equals invoice.Invoice_Id
                where ids.Contains(detail.Payment_Id) && detail.DeletedAt == null
                orderby detail.PaymentDetailID
                select new PaymentDetailDTO
                {
                    PaymentDetailID = detail.PaymentDetailID,
                    PaymentDetail_Id = detail.PaymentDetail_Id,
                    Payment_Id = detail.Payment_Id,
                    Invoice_Id = detail.Invoice_Id,
                    InvoiceNo = invoice.InvoiceNo,
                    InvoiceDate = invoice.InvoiceDate,
                    InvoiceAmount = detail.InvoiceAmount,
                    PreviousPaidAmount = detail.PreviousPaidAmount,
                    PaidAmount = detail.PaidAmount,
                    BalanceAmount = detail.BalanceAmount,
                    Remarks = detail.Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    DeletedBy = detail.DeletedBy,
                    DeletedAt = detail.DeletedAt
                }).ToListAsync();
        }

        private async Task<List<PaymentChargeDTO>> GetCharges(IEnumerable<Guid> paymentIds)
        {
            var ids = paymentIds.ToList();
            if (ids.Count == 0)
            {
                return new List<PaymentChargeDTO>();
            }

            return await (
                from paymentCharge in _dbContext.PaymentCharges.AsNoTracking()
                join charge in _dbContext.Charge.AsNoTracking()
                    on paymentCharge.Charge_Id equals charge.Charge_Id
                where ids.Contains(paymentCharge.Payment_Id) && paymentCharge.DeletedAt == null
                orderby paymentCharge.Id
                select new PaymentChargeDTO
                {
                    Id = paymentCharge.Id,
                    Payment_Charge_Id = paymentCharge.Payment_Charge_Id,
                    Charge_Id = paymentCharge.Charge_Id,
                    ChargeName = charge.ChargeName ?? string.Empty,
                    Payment_Id = paymentCharge.Payment_Id,
                    AddLessType = paymentCharge.AddLessType,
                    Percentage = paymentCharge.Percentage,
                    Amount = paymentCharge.Amount,
                    Remarks = paymentCharge.Remarks,
                    CreatedBy = paymentCharge.CreatedBy,
                    CreatedAt = paymentCharge.CreatedAt,
                    UpdatedBy = paymentCharge.UpdatedBy,
                    UpdatedAt = paymentCharge.UpdatedAt,
                    DeletedBy = paymentCharge.DeletedBy,
                    DeletedAt = paymentCharge.DeletedAt
                }).ToListAsync();
        }

        private async Task SetPaymentAmounts(
            PaymentMasterDTO payment,
            List<PaymentDetailDTO> details,
            Guid? currentPaymentId)
        {
            if (details.Count == 0)
            {
                throw new ArgumentException("At least one invoice payment detail is required.");
            }

            var duplicateInvoice = details.GroupBy(detail => detail.Invoice_Id)
                .Any(group => group.Key == Guid.Empty || group.Count() > 1);
            if (duplicateInvoice)
            {
                throw new ArgumentException("Each invoice can appear only once in a payment.");
            }

            if (details.Any(detail => detail.PaidAmount <= 0))
            {
                throw new ArgumentException("PaidAmount must be greater than zero for every invoice.");
            }

            var availableInvoices = await GetAvailableInvoices(payment.Supplier_ID, currentPaymentId);
            var availableById = availableInvoices.ToDictionary(invoice => invoice.Invoice_Id);
            foreach (var detail in details)
            {
                if (!availableById.TryGetValue(detail.Invoice_Id, out var invoice))
                {
                    throw new ArgumentException($"Invoice {detail.Invoice_Id} is not available for this supplier.");
                }

                if (detail.PaidAmount > invoice.BalanceAmount)
                {
                    throw new ArgumentException($"PaidAmount exceeds the outstanding balance for invoice {invoice.InvoiceNo}.");
                }

                detail.InvoiceAmount = invoice.InvoiceAmount;
                detail.PreviousPaidAmount = invoice.PreviousPaidAmount;
                detail.BalanceAmount = invoice.BalanceAmount - detail.PaidAmount;
            }

            payment.PaymentAmount = details.Sum(detail => detail.PaidAmount);
            if (payment.DeductionAmount < 0 || payment.DeductionAmount > payment.PaymentAmount)
            {
                throw new ArgumentException("DeductionAmount must be between zero and PaymentAmount.");
            }

            payment.NetAmount = payment.PaymentAmount - payment.DeductionAmount;
        }

        private async Task AddDetails(Guid paymentId, List<PaymentDetailDTO> details, int? createdBy, DateTime now)
        {
            var auditUser = createdBy?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var detail in details)
            {
                detail.PaymentDetailID = 0;
                detail.PaymentDetail_Id = detail.PaymentDetail_Id == Guid.Empty
                    ? Guid.NewGuid()
                    : detail.PaymentDetail_Id;
                detail.Payment_Id = paymentId;
                detail.CreatedBy = string.IsNullOrWhiteSpace(detail.CreatedBy) ? auditUser : detail.CreatedBy;
                detail.CreatedAt ??= now;
                detail.UpdatedBy = null;
                detail.UpdatedAt = null;
                detail.DeletedBy = null;
                detail.DeletedAt = null;
            }

            await _dbContext.PaymentDetails.AddRangeAsync(details);
        }

        private async Task ReplaceDetails(Guid paymentId, List<PaymentDetailDTO> incoming, int? updatedBy, DateTime now)
        {
            var existing = await _dbContext.PaymentDetails
                .Where(detail => detail.Payment_Id == paymentId)
                .ToListAsync();
            var incomingIds = incoming
                .Where(detail => detail.PaymentDetail_Id != Guid.Empty)
                .Select(detail => detail.PaymentDetail_Id)
                .ToHashSet();
            if (incomingIds.Count != incoming.Count(detail => detail.PaymentDetail_Id != Guid.Empty))
            {
                throw new ArgumentException("Payment detail identifiers must be unique.");
            }

            var auditUser = updatedBy?.ToString(CultureInfo.InvariantCulture);

            foreach (var detail in existing)
            {
                if (detail.DeletedAt == null && !incomingIds.Contains(detail.PaymentDetail_Id))
                {
                    detail.DeletedBy = auditUser;
                    detail.DeletedAt = now;
                }
            }

            foreach (var item in incoming)
            {
                var detail = item.PaymentDetail_Id == Guid.Empty
                    ? null
                    : existing.FirstOrDefault(existingDetail =>
                        existingDetail.PaymentDetail_Id == item.PaymentDetail_Id);

                if (detail == null)
                {
                    item.PaymentDetailID = 0;
                    item.PaymentDetail_Id = Guid.NewGuid();
                    item.Payment_Id = paymentId;
                    item.CreatedBy = string.IsNullOrWhiteSpace(item.CreatedBy)
                        ? auditUser ?? string.Empty
                        : item.CreatedBy;
                    item.CreatedAt ??= now;
                    item.UpdatedBy = null;
                    item.UpdatedAt = null;
                    item.DeletedBy = null;
                    item.DeletedAt = null;
                    _dbContext.PaymentDetails.Add(item);
                    continue;
                }

                detail.Invoice_Id = item.Invoice_Id;
                detail.InvoiceAmount = item.InvoiceAmount;
                detail.PreviousPaidAmount = item.PreviousPaidAmount;
                detail.PaidAmount = item.PaidAmount;
                detail.Remarks = item.Remarks;
                detail.UpdatedBy = auditUser;
                detail.UpdatedAt = now;
                detail.DeletedBy = null;
                detail.DeletedAt = null;
            }
        }

        private async Task AddCharges(Guid paymentId, List<PaymentChargeDTO> charges, int? createdBy, DateTime now)
        {
            var auditUser = createdBy?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var charge in charges)
            {
                charge.Id = 0;
                charge.Payment_Charge_Id = charge.Payment_Charge_Id == Guid.Empty
                    ? Guid.NewGuid()
                    : charge.Payment_Charge_Id;
                charge.Payment_Id = paymentId;
                charge.CreatedBy = string.IsNullOrWhiteSpace(charge.CreatedBy) ? auditUser : charge.CreatedBy;
                charge.CreatedAt ??= now;
                charge.UpdatedBy = null;
                charge.UpdatedAt = null;
                charge.DeletedBy = null;
                charge.DeletedAt = null;
            }

            await _dbContext.PaymentCharges.AddRangeAsync(charges);
        }

        private async Task ReplaceCharges(Guid paymentId, List<PaymentChargeDTO> incoming, int? updatedBy, DateTime now)
        {
            var existing = await _dbContext.PaymentCharges
                .Where(charge => charge.Payment_Id == paymentId)
                .ToListAsync();
            var incomingIds = incoming
                .Where(charge => charge.Payment_Charge_Id != Guid.Empty)
                .Select(charge => charge.Payment_Charge_Id)
                .ToHashSet();
            if (incomingIds.Count != incoming.Count(charge => charge.Payment_Charge_Id != Guid.Empty))
            {
                throw new ArgumentException("Payment charge identifiers must be unique.");
            }

            var auditUser = updatedBy?.ToString(CultureInfo.InvariantCulture);
            foreach (var charge in existing)
            {
                if (charge.DeletedAt == null && !incomingIds.Contains(charge.Payment_Charge_Id))
                {
                    charge.DeletedBy = auditUser;
                    charge.DeletedAt = now;
                }
            }

            foreach (var item in incoming)
            {
                var charge = item.Payment_Charge_Id == Guid.Empty
                    ? null
                    : existing.FirstOrDefault(existingCharge =>
                        existingCharge.Payment_Charge_Id == item.Payment_Charge_Id);

                if (charge == null)
                {
                    item.Id = 0;
                    item.Payment_Charge_Id = Guid.NewGuid();
                    item.Payment_Id = paymentId;
                    item.CreatedBy = string.IsNullOrWhiteSpace(item.CreatedBy)
                        ? auditUser ?? string.Empty
                        : item.CreatedBy;
                    item.CreatedAt ??= now;
                    item.UpdatedBy = null;
                    item.UpdatedAt = null;
                    item.DeletedBy = null;
                    item.DeletedAt = null;
                    _dbContext.PaymentCharges.Add(item);
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
