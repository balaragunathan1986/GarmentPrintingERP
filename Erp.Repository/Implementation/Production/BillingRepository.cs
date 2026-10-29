using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class BillingRepository : IBillingRepository
    {
        private readonly AppDBContext _dbContext;

        public BillingRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.BillingMaster
                .OrderByDescending(x => x.Billing_Number)
                .Select(x => x.Billing_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.Billing, Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<BillingListDTO>> GetBillingList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var query =
                from billing in _dbContext.BillingMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking() on billing.Supplier_Id equals supplier.Id
                where billing.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || billing.Billing_Number.Contains(searchText)
                        || (supplier.Supplier_Name ?? string.Empty).Contains(searchText))
                    && (!fromDate.HasValue || billing.Billing_Date >= fromDate)
                    && (!toDate.HasValue || billing.Billing_Date < toDate.Value.Date.AddDays(1))
                orderby billing.Id descending
                select new BillingListDTO
                {
                    Id = billing.Id,
                    Billing_Id = billing.Billing_Id,
                    Billing_Number = billing.Billing_Number,
                    Billing_Date = billing.Billing_Date,
                    Supplier_Id = billing.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name ?? string.Empty,
                    Total_Bill_Qty = billing.Total_Bill_Qty,
                    Total_Bill_Amount = billing.Total_Bill_Amount,
                    Payment_Status = billing.Payment_Status
                };

            return await query.ToListAsync();
        }

        public async Task<BillingMasterDTO> GetBilling(Guid billingId)
        {
            var billing = await (
                from billingMaster in _dbContext.BillingMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking() on billingMaster.Supplier_Id equals supplier.Id
                where billingMaster.Billing_Id == billingId && billingMaster.DeletedAt == null
                select new BillingMasterDTO
                {
                    Id = billingMaster.Id,
                    Billing_Id = billingMaster.Billing_Id,
                    Billing_Number = billingMaster.Billing_Number,
                    Billing_Date = billingMaster.Billing_Date,
                    Supplier_Id = billingMaster.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name ?? string.Empty,
                    Company_Id = billingMaster.Company_Id,
                    Total_Bill_Qty = billingMaster.Total_Bill_Qty,
                    Total_Bill_Amount = billingMaster.Total_Bill_Amount,
                    Payment_Status = billingMaster.Payment_Status,
                    Remarks = billingMaster.Remarks
                }).FirstOrDefaultAsync();

            if (billing == null)
                return new BillingMasterDTO();

            billing.Billing_Details = await _dbContext.BillingDetails
                .Where(x => x.Billing_Id == billingId)
                .AsNoTracking()
                .ToListAsync();

            return billing;
        }

        public async Task<BillingReportDTO?> GetBillingReport(Guid billingId)
        {
            var billing = await (
                from billingMaster in _dbContext.BillingMaster.AsNoTracking()

                join company in _dbContext.Company.AsNoTracking()
                    on billingMaster.Company_Id equals company.Id

                join supplier in _dbContext.Supplier.AsNoTracking()
                    on billingMaster.Supplier_Id equals supplier.Id

                where billingMaster.Billing_Id == billingId

                select new
                {
                    Billing = billingMaster,
                    Supplier = supplier,
                    Company = company
                }
            ).FirstOrDefaultAsync();

            if (billing == null)
            {
                return null;
            }

            var deductions = await _dbContext.BillingDetails
                .AsNoTracking()
                .Where(x => x.Billing_Id == billingId)
                .ToListAsync();

            // =====================================================
            // TOTALS
            // =====================================================

            var totalDebitQty =
                deductions.Sum(x => x.Net_Bill_Qty);

            var totalDebitAmount =
                deductions.Sum(x => x.Bill_Amount);

            var netQty =
                billing.Billing.Total_Bill_Qty - totalDebitQty;

            var netAmount =
                billing.Billing.Total_Bill_Amount - totalDebitAmount;

            // =====================================================
            // DEBIT DETAILS
            // =====================================================

            var debitDetails =
                deductions
                    .Select((x, index) => new BillingDebitDetailDTO
                    {
                        SlNo = index + 1,

                        Deduction = "Coconut charcoal",

                        HSNCode = "44029010",

                        Quantity = x.Net_Bill_Qty,

                        Uom = "Kg",

                        Rate = x.Rate_Per_Kg,

                        Amount = x.Bill_Amount
                    })
                    .ToList();

            // =====================================================
            // RESULT
            // =====================================================

            return new BillingReportDTO
            {
                Billing_Id =
                    billing.Billing.Billing_Id,

                // =================================================
                // COMPANY
                // =================================================

                CompanyName =
                    billing.Company.Company_Name ?? string.Empty,

                CompanyAddress =
                    billing.Company.Address1 ?? string.Empty,

                CompanyGSTIN =
                    billing.Company.Gst_Number ?? string.Empty,

                CompanyPhone =
                    billing.Company.Phone_Number ?? string.Empty,

                CompanyState =
                    billing.Company.Company_State ?? string.Empty,

                // =================================================
                // SUPPLIER
                // =================================================

                SupplierName =
                    billing.Supplier.Supplier_Name ?? string.Empty,

                SupplierAddress =
                    billing.Supplier.Address1 ?? string.Empty,

                SupplierGSTIN =
                    billing.Supplier.Gst_Number ?? string.Empty,

                SupplierPhone =
                    billing.Supplier.Phone_Number ?? string.Empty,

                SupplierState =
                    billing.Supplier.Supplier_State ?? string.Empty,

                // =================================================
                // BILLING
                // =================================================

                ReceiptNumber =
                    billing.Billing.Billing_Number,

                ReceiptDate =
                    billing.Billing.Billing_Date ?? DateTime.UtcNow,

                BillingNumber =
                    billing.Billing.Billing_Number,

                BillingAmount =
                    billing.Billing.Total_Bill_Amount,

                // =================================================
                // SOURCE BILL
                // =================================================

                SourceBillQty =
                    billing.Billing.Total_Bill_Qty,

                SourceBillUom =
                    "Kg",

                // =================================================
                // DEBIT DETAILS
                // =================================================

                DebitDetails =
                    debitDetails,

                TotalDebitQty =
                    totalDebitQty,

                // =================================================
                // NET VALUES
                // =================================================

                NetQty =
                    netQty,

                NetAmount =
                    netAmount,

                // =================================================
                // AMOUNT IN WORDS
                // =================================================

                AmountInWords =
                    ConvertAmountToWords(totalDebitAmount),

                // =================================================
                // REMARKS
                // =================================================

                Remarks =
                    "Thank you for doing business with us."
            };
        }
        private static string ConvertAmountToWords(decimal amount)
        {
            if (amount < 0)
                return "Minus " + ConvertAmountToWords(Math.Abs(amount));

            long rupees = (long)Math.Floor(amount);

            int paise = (int)Math.Round(
                (amount - rupees) * 100,
                MidpointRounding.AwayFromZero);

            string result = NumberToWords(rupees) + " Rupees";

            if (paise > 0)
            {
                result += " and " + NumberToWords(paise) + " Paise";
            }

            return result + " Only";
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "Zero";

            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));

            string words = string.Empty;

            if (number >= 10000000)
            {
                words += NumberToWords(number / 10000000)
                         + " Crore ";

                number %= 10000000;
            }

            if (number >= 100000)
            {
                words += NumberToWords(number / 100000)
                         + " Lakh ";

                number %= 100000;
            }

            if (number >= 1000)
            {
                words += NumberToWords(number / 1000)
                         + " Thousand ";

                number %= 1000;
            }

            if (number >= 100)
            {
                words += NumberToWords(number / 100)
                         + " Hundred ";

                number %= 100;
            }

            if (number > 0)
            {
                if (words != string.Empty)
                    words += "and ";

                if (number < 20)
                {
                    words += Units[(int)number];
                }
                else
                {
                    words += Tens[(int)number / 10];

                    if ((number % 10) > 0)
                        words += " " + Units[(int)(number % 10)];
                }
            }

            return words.Trim();
        }

        private static readonly string[] Units =
        {
            "Zero",
            "One",
            "Two",
            "Three",
            "Four",
            "Five",
            "Six",
            "Seven",
            "Eight",
            "Nine",
            "Ten",
            "Eleven",
            "Twelve",
            "Thirteen",
            "Fourteen",
            "Fifteen",
            "Sixteen",
            "Seventeen",
            "Eighteen",
            "Nineteen"
        };

        private static readonly string[] Tens =
        {
            "",
            "",
            "Twenty",
            "Thirty",
            "Forty",
            "Fifty",
            "Sixty",
            "Seventy",
            "Eighty",
            "Ninety"
        };

        public async Task<BillingModel> Save(BillingModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                model.Company_Id = 1;
                if (model.Billing_Id == Guid.Empty)
                    model.Billing_Id = Guid.NewGuid();

                var validateNumber = await GenerateNumber();
                if (validateNumber != null) {
                    model.Billing_Number = validateNumber == model.Billing_Number ? model.Billing_Number : validateNumber;
                }

                var billing = new BillingMasterDTO
                {
                    Billing_Id = model.Billing_Id,
                    Billing_Number = model.Billing_Number,
                    Billing_Date = model.Billing_Date,
                    Supplier_Id = model.Supplier_Id,
                    Company_Id = model.Company_Id,
                    Total_Bill_Qty = model.Total_Bill_Qty,
                    Total_Bill_Amount = model.Total_Bill_Amount,
                    Payment_Status = model.Payment_Status,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbContext.BillingMaster.AddAsync(billing);
                await _dbContext.SaveChangesAsync();

                if (model.Billing_Details.Any())
                {
                    var details = model.Billing_Details.Select(detail => new BillingDetailDTO
                    {
                        Billing_Detail_Id = Guid.NewGuid(),
                        Billing_Id = model.Billing_Id,
                        Production_Despatch_Id = detail.Production_Despatch_Id,
                        Production_Despatch_Number = detail.Production_Despatch_Number,
                        Despatch_Date = detail.Despatch_Date,
                        Net_Bill_Qty = detail.Net_Bill_Qty,
                        Rate_Per_Kg = detail.Rate_Per_Kg,
                        Bill_Amount = detail.Bill_Amount,
                        CreatedBy = model.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _dbContext.BillingDetails.AddRangeAsync(details);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                model.Id = billing.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BillingModel> Update(BillingModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Billing_Id == Guid.Empty)
                    throw new ArgumentException("Billing Id is required.");

                var billing = await _dbContext.BillingMaster
                    .FirstOrDefaultAsync(x => x.Billing_Id == model.Billing_Id && x.DeletedAt == null);

                if (billing == null)
                    throw new KeyNotFoundException("Billing not found.");

                billing.Billing_Number = model.Billing_Number;
                billing.Billing_Date = model.Billing_Date;
                billing.Supplier_Id = model.Supplier_Id;
                billing.Company_Id = 1;
                billing.Total_Bill_Qty = model.Total_Bill_Qty;
                billing.Total_Bill_Amount = model.Total_Bill_Amount;
                billing.Payment_Status = model.Payment_Status;
                billing.Remarks = model.Remarks;
                billing.UpdatedBy = model.UpdatedBy;
                billing.UpdatedAt = DateTime.UtcNow;

                var existingDetails = await _dbContext.BillingDetails
                    .Where(x => x.Billing_Id == model.Billing_Id)
                    .ToListAsync();

                if (existingDetails.Any())
                    _dbContext.BillingDetails.RemoveRange(existingDetails);

                if (model.Billing_Details.Any())
                {
                    var details = model.Billing_Details.Select(detail => new BillingDetailDTO
                    {
                        Billing_Detail_Id = Guid.NewGuid(),
                        Billing_Id = model.Billing_Id,
                        Production_Despatch_Id = detail.Production_Despatch_Id,
                        Production_Despatch_Number = detail.Production_Despatch_Number,
                        Despatch_Date = detail.Despatch_Date,
                        Net_Bill_Qty = detail.Net_Bill_Qty,
                        Rate_Per_Kg = detail.Rate_Per_Kg,
                        Bill_Amount = detail.Bill_Amount,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await _dbContext.BillingDetails.AddRangeAsync(details);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                model.Id = billing.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid billingId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var billing = await _dbContext.BillingMaster.FirstOrDefaultAsync(x => x.Billing_Id == billingId);
                if (billing == null)
                    return false;

                await _dbContext.BillingDetails
                    .Where(x => x.Billing_Id == billingId)
                    .ExecuteDeleteAsync();

                _dbContext.BillingMaster.Remove(billing);
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