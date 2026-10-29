using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class PaymentVoucherRepository : IPaymentVoucherRepository
    {
        private readonly AppDBContext _dbContext;

        public PaymentVoucherRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.PaymentVoucher
                .OrderByDescending(x => x.Payment_Voucher_Number)
                .Select(x => x.Payment_Voucher_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.PaymentVoucher,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber)
            );
        }

        public async Task<List<PaymentVoucherListDTO>> GetPaymentVoucherList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var query =
                from voucher in _dbContext.PaymentVoucher.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on voucher.Supplier_Id equals supplier.Id into supplierJoin
                from supplier in supplierJoin.DefaultIfEmpty()
                where voucher.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || voucher.Payment_Voucher_Number.Contains(searchText)
                        || (supplier != null && supplier.Supplier_Name != null && supplier.Supplier_Name.Contains(searchText)))
                    && (!fromDate.HasValue || voucher.Payment_Voucher_Date >= fromDate)
                    && (!toDate.HasValue || voucher.Payment_Voucher_Date < toDate.Value.Date.AddDays(1))
                orderby voucher.Id descending
                select new PaymentVoucherListDTO
                {
                    Id = voucher.Id,
                    Payment_Voucher_Id = voucher.Payment_Voucher_Id,
                    Payment_Voucher_Number = voucher.Payment_Voucher_Number,
                    Payment_Voucher_Date = voucher.Payment_Voucher_Date,
                    Supplier_Id = voucher.Supplier_Id,
                    Supplier_Name = supplier != null ? supplier.Supplier_Name : null,
                    Total_Amount = voucher.Total_Amount,
                    Reference_Number = voucher.Reference_Number,
                    Status = voucher.Status
                };

            return await query.ToListAsync();
        }

        public async Task<PaymentVoucherDTO> GetPaymentVoucher(Guid paymentVoucherId)
        {
            return await (
                from voucher in _dbContext.PaymentVoucher.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on voucher.Supplier_Id equals supplier.Id into supplierJoin
                from supplier in supplierJoin.DefaultIfEmpty()
                where voucher.Payment_Voucher_Id == paymentVoucherId && voucher.DeletedAt == null
                select new PaymentVoucherDTO
                {
                    Id = voucher.Id,
                    Payment_Voucher_Id = voucher.Payment_Voucher_Id,
                    Payment_Voucher_Number = voucher.Payment_Voucher_Number,
                    Payment_Voucher_Date = voucher.Payment_Voucher_Date,
                    Company_Id = voucher.Company_Id,
                    Supplier_Id = voucher.Supplier_Id,
                    Supplier_Name = supplier != null ? supplier.Supplier_Name : null,
                    PaymentMode_Id = voucher.PaymentMode_Id,
                    PaymentTerms_Id = voucher.PaymentTerms_Id,
                    Reference_Number = voucher.Reference_Number,
                    Total_Amount = voucher.Total_Amount,
                    Status = voucher.Status,
                    Remarks = voucher.Remarks,
                    CreatedBy = voucher.CreatedBy,
                    CreatedAt = voucher.CreatedAt,
                    UpdatedBy = voucher.UpdatedBy,
                    UpdatedAt = voucher.UpdatedAt
                }).FirstOrDefaultAsync() ?? new PaymentVoucherDTO();
        }

        public async Task<PaymentVoucherModel> Save(PaymentVoucherModel model)
        {
            model.Company_Id = 1;

            if (model.Payment_Voucher_Id == Guid.Empty)
                model.Payment_Voucher_Id = Guid.NewGuid();

            var validateNumber = await GenerateNumber();
            if (validateNumber != null)
            {
                model.Payment_Voucher_Number = validateNumber == model.Payment_Voucher_Number
                    ? model.Payment_Voucher_Number
                    : validateNumber;
            }

            var voucher = new PaymentVoucherDTO
            {
                Payment_Voucher_Id = model.Payment_Voucher_Id,
                Payment_Voucher_Number = model.Payment_Voucher_Number,
                Payment_Voucher_Date = model.Payment_Voucher_Date,
                Company_Id = model.Company_Id,
                Supplier_Id = model.Supplier_Id,
                PaymentMode_Id = model.PaymentMode_Id,
                PaymentTerms_Id = model.PaymentTerms_Id,
                Reference_Number = model.Reference_Number,
                Total_Amount = model.Total_Amount,
                Status = string.IsNullOrWhiteSpace(model.Status) ? "Draft" : model.Status,
                Remarks = model.Remarks,
                CreatedBy = model.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.PaymentVoucher.AddAsync(voucher);
            await _dbContext.SaveChangesAsync();

            model.Id = voucher.Id;
            return model;
        }

        public async Task<PaymentVoucherModel> Update(PaymentVoucherModel model)
        {
            if (model.Payment_Voucher_Id == Guid.Empty)
                throw new ArgumentException("Payment Voucher Id is required.");

            var voucher = await _dbContext.PaymentVoucher.FirstOrDefaultAsync(x =>
                x.Payment_Voucher_Id == model.Payment_Voucher_Id && x.DeletedAt == null);

            if (voucher == null)
                throw new KeyNotFoundException("Payment Voucher not found.");

            voucher.Payment_Voucher_Number = model.Payment_Voucher_Number;
            voucher.Payment_Voucher_Date = model.Payment_Voucher_Date;
            voucher.Company_Id = 1;
            voucher.Supplier_Id = model.Supplier_Id;
            voucher.PaymentMode_Id = model.PaymentMode_Id;
            voucher.PaymentTerms_Id = model.PaymentTerms_Id;
            voucher.Reference_Number = model.Reference_Number;
            voucher.Total_Amount = model.Total_Amount;
            voucher.Status = string.IsNullOrWhiteSpace(model.Status) ? voucher.Status : model.Status;
            voucher.Remarks = model.Remarks;
            voucher.UpdatedBy = model.UpdatedBy;
            voucher.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            model.Id = voucher.Id;
            return model;
        }

        public async Task<bool> Delete(Guid paymentVoucherId)
        {
            var voucher = await _dbContext.PaymentVoucher
                .FirstOrDefaultAsync(x => x.Payment_Voucher_Id == paymentVoucherId);

            if (voucher == null)
                return false;

            _dbContext.PaymentVoucher.Remove(voucher);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
