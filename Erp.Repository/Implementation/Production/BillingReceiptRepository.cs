using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class BillingReceiptRepository : IBillingReceiptRepository
    {
        private readonly AppDBContext _dbContext;

        public BillingReceiptRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.BillingReceiptMaster
                .OrderByDescending(x => x.Billing_Receipt_Number)
                .Select(x => x.Billing_Receipt_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.BillingReceipt, Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<BillingReceiptListDTO>> GetBillingReceiptList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var paymentModes = await _dbContext.PaymentMode.Where(itm => !itm.IsDeleted).ToListAsync();
            var query =
                from receipt in _dbContext.BillingReceiptMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking() on receipt.Supplier_Id equals supplier.Id
                join billing in _dbContext.BillingMaster.AsNoTracking() on receipt.Billing_Id equals billing.Billing_Id
                where receipt.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || (receipt.Billing_Receipt_Number ?? string.Empty).Contains(searchText)
                        || receipt.Billing_Number.Contains(searchText)
                        || (supplier.Supplier_Name ?? string.Empty).Contains(searchText))
                    && (!fromDate.HasValue || receipt.Billing_Receipt_Date >= fromDate)
                    && (!toDate.HasValue || receipt.Billing_Receipt_Date < toDate.Value.Date.AddDays(1))
                orderby receipt.Id descending
                select new BillingReceiptListDTO
                {
                    Id = receipt.Id,
                    Billing_Receipt_Id = receipt.Billing_Receipt_Id,
                    Billing_Id = receipt.Billing_Id,
                    Billing_Number = receipt.Billing_Number,
                    Supplier_Id = receipt.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name ?? string.Empty,
                    Billing_Receipt_Number = receipt.Billing_Receipt_Number,
                    Billing_Receipt_Date = receipt.Billing_Receipt_Date,
                    Billing_Amount = receipt.Billing_Amount,
                    Source_Bill_Qty = billing.Total_Bill_Qty,
                    Total_Debit_Quantity = receipt.Total_Debit_Quantity,
                    Net_Quantity = receipt.Net_Quantity,
                    Net_Amount = receipt.Net_Amount,
                    Receipt_Amount = receipt.Receipt_Amount ?? 0,
                    Receipt_ref_Number = receipt.Receipt_Ref_Number ?? "",       
                    Payment_Type =  ""
                };

            return await query.ToListAsync();
        }

        public async Task<BillingReceiptMasterDTO> GetBillingReceipt(Guid billingReceiptId)
        {
            var receipt = await (
                from receiptMaster in _dbContext.BillingReceiptMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking() on receiptMaster.Supplier_Id equals supplier.Id
                join billing in _dbContext.BillingMaster.AsNoTracking() on receiptMaster.Billing_Id equals billing.Billing_Id
                where receiptMaster.Billing_Receipt_Id == billingReceiptId && receiptMaster.DeletedAt == null
                select new BillingReceiptMasterDTO
                {
                    Id = receiptMaster.Id,
                    Billing_Receipt_Id = receiptMaster.Billing_Receipt_Id,
                    Billing_Id = receiptMaster.Billing_Id,
                    Billing_Number = receiptMaster.Billing_Number,
                    Company_Id = receiptMaster.Company_Id,
                    Supplier_Id = receiptMaster.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name ?? string.Empty,
                    Billing_Receipt_Number = receiptMaster.Billing_Receipt_Number,
                    Billing_Receipt_Date = receiptMaster.Billing_Receipt_Date,
                    Billing_Amount = receiptMaster.Billing_Amount,
                    Source_Bill_Qty = billing.Total_Bill_Qty,
                    Total_Debit_Quantity = receiptMaster.Total_Debit_Quantity,
                    Net_Quantity = receiptMaster.Net_Quantity,
                    Net_Amount = receiptMaster.Net_Amount,
                    Receipt_Amount = receiptMaster.Receipt_Amount,
                    Receipt_Ref_Number = receiptMaster.Receipt_Ref_Number,
                    payment_Type = receiptMaster.payment_Type
                }).FirstOrDefaultAsync();

            if (receipt == null)
                return new BillingReceiptMasterDTO();

            receipt.Billing_Receipt_Details = await _dbContext.BillingReceiptDetails
                .Where(x => x.Billing_Receipt_Id == billingReceiptId && x.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync();

            return receipt;
        }

        public async Task<BillingReceiptModel> Save(BillingReceiptModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                model.Company_Id = 1;
                if (model.Billing_Receipt_Id == Guid.Empty)
                    model.Billing_Receipt_Id = Guid.NewGuid();

                var validateNumber = await GenerateNumber();
                if (validateNumber != null)
                {
                    model.Billing_Number = validateNumber == model.Billing_Number ? model.Billing_Number : validateNumber;
                }

                var receipt = new BillingReceiptMasterDTO
                {
                    Billing_Receipt_Id = model.Billing_Receipt_Id,
                    Billing_Id = model.Billing_Id,
                    Billing_Number = model.Billing_Number,
                    Company_Id = model.Company_Id,
                    Supplier_Id = model.Supplier_Id,
                    Billing_Receipt_Number = model.Billing_Receipt_Number,
                    Billing_Receipt_Date = model.Billing_Receipt_Date,
                    Billing_Amount = model.Billing_Amount,
                    Total_Debit_Quantity = model.Total_Debit_Quantity,
                    Net_Quantity = model.Net_Quantity,
                    Net_Amount = model.Net_Amount,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    Receipt_Amount = model.Receipt_Amount,
                    payment_Type = model.payment_Type,
                    Receipt_Ref_Number = model.Receipt_Ref_Number,
                };

                await _dbContext.BillingReceiptMaster.AddAsync(receipt);
                await _dbContext.SaveChangesAsync();
                await AddDetails(model, model.Billing_Receipt_Id, model.CreatedBy);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                model.Id = receipt.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BillingReceiptModel> Update(BillingReceiptModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Billing_Receipt_Id == Guid.Empty)
                    throw new ArgumentException("Billing receipt Id is required.");

                var receipt = await _dbContext.BillingReceiptMaster
                    .FirstOrDefaultAsync(x => x.Billing_Receipt_Id == model.Billing_Receipt_Id && x.DeletedAt == null);

                if (receipt == null)
                    throw new KeyNotFoundException("Billing receipt not found.");

                receipt.Billing_Id = model.Billing_Id;
                receipt.Billing_Number = model.Billing_Number;
                receipt.Supplier_Id = model.Supplier_Id;
                receipt.Company_Id = 1;
                receipt.Billing_Receipt_Number = model.Billing_Receipt_Number;
                receipt.Billing_Receipt_Date = model.Billing_Receipt_Date;
                receipt.Billing_Amount = model.Billing_Amount;
                receipt.Total_Debit_Quantity = model.Total_Debit_Quantity;
                receipt.Net_Quantity = model.Net_Quantity;
                receipt.Net_Amount = model.Net_Amount;
                receipt.UpdatedBy = model.UpdatedBy;
                receipt.UpdatedAt = DateTime.UtcNow;
                receipt.Receipt_Amount = model.Receipt_Amount;
                receipt.Receipt_Ref_Number = model.Receipt_Ref_Number;
                receipt.payment_Type = model.payment_Type;

                var existingDetails = await _dbContext.BillingReceiptDetails
                    .Where(x => x.Billing_Receipt_Id == model.Billing_Receipt_Id)
                    .ToListAsync();

                if (existingDetails.Any())
                    _dbContext.BillingReceiptDetails.RemoveRange(existingDetails);

                await AddDetails(model, model.Billing_Receipt_Id, model.UpdatedBy ?? model.CreatedBy);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                model.Id = receipt.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid billingReceiptId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var receipt = await _dbContext.BillingReceiptMaster
                    .FirstOrDefaultAsync(x => x.Billing_Receipt_Id == billingReceiptId && x.DeletedAt == null);

                if (receipt == null)
                    return false;

                await _dbContext.BillingReceiptDetails
                    .Where(x => x.Billing_Receipt_Id == billingReceiptId)
                    .ExecuteDeleteAsync();

                _dbContext.BillingReceiptMaster.Remove(receipt);
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

        private async Task AddDetails(BillingReceiptModel model, Guid billingReceiptId, string? createdBy)
        {
            if (!model.Billing_Receipt_Details.Any())
                return;

            var details = model.Billing_Receipt_Details.Select(detail => new BillingReceiptDetailDTO
            {
                Billing_Receipt_Detail_Id = Guid.NewGuid(),
                Billing_Receipt_Id = billingReceiptId,
                Deduction_Type = detail.Deduction_Type,
                Deduction_Quantity = detail.Deduction_Quantity,
                Deduction_Percentage = detail.Deduction_Percentage,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _dbContext.BillingReceiptDetails.AddRangeAsync(details);
        }
    }
}