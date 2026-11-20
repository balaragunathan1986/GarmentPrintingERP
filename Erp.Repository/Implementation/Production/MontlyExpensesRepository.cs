using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class MontlyExpensesRepository : IMontlyExpensesRepository
    {
        private readonly AppDBContext _dbContext;

        public MontlyExpensesRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.MontlyExpenses
                .OrderByDescending(x => x.MontlyExpenses_Number)
                .Select(x => x.MontlyExpenses_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.MontlyExpenses,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<MontlyExpensesDTO>> GetList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            var query = CreateExpenseQuery();

            if (fromDate.HasValue)
                query = query.Where(x => x.MontlyExpenses >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(x => x.MontlyExpenses < toDate.Value.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var term = searchText.Trim();
                query = query.Where(x =>
                    x.MontlyExpenses_Number.Contains(term) ||
                    x.CompanyName.Contains(term) ||
                    x.StyleName.Contains(term) ||
                    x.JobOrderNumber.Contains(term));
            }

            var expenses = await query
                .OrderByDescending(x => x.Id)
                .AsNoTracking()
                .ToListAsync();

            await LoadDetails(expenses);
            return expenses;
        }

        public async Task<MontlyExpensesDTO?> Get(Guid montlyExpensesId)
        {
            var expense = await CreateExpenseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MontlyExpenses_Id == montlyExpensesId);

            if (expense == null)
                return null;

            await LoadDetails(new List<MontlyExpensesDTO> { expense });
            return expense;
        }

        public async Task<MontlyExpensesModel> Save(MontlyExpensesModel model)
        {
            Validate(model);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.MontlyExpenses_Id == Guid.Empty)
                    model.MontlyExpenses_Id = Guid.NewGuid();

                var generatedNumber = await GenerateNumber();
                if (model.MontlyExpenses_Number != generatedNumber)
                    model.MontlyExpenses_Number = generatedNumber;

                var now = DateTime.UtcNow;
                var expense = new MontlyExpensesDTO
                {
                    MontlyExpenses_Id = model.MontlyExpenses_Id,
                    MontlyExpenses_Number = model.MontlyExpenses_Number,
                    MontlyExpenses = model.MontlyExpenses,
                    Company_Id = model.Company_Id,
                    StyleId = model.StyleId,
                    JobOrder_Id = model.JobOrder_Id,
                    Remarks = model.Remarks,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = now
                };

                await _dbContext.MontlyExpenses.AddAsync(expense);
                await _dbContext.SaveChangesAsync();

                var details = model.MontlyExpenses_Details.Select(detail => new MontlyExpensesDetailDTO
                {
                    MontlyExpenses_Details_Id = detail.MontlyExpenses_Details_Id == Guid.Empty
                        ? Guid.NewGuid()
                        : detail.MontlyExpenses_Details_Id,
                    MontlyExpenses_Id = model.MontlyExpenses_Id,
                    Charge_Id = detail.Charge_Id,
                    AddLessType = detail.AddLessType,
                    Percentage = detail.Percentage,
                    Amount = detail.Amount,
                    Remarks = detail.Remarks,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = now
                }).ToList();

                await _dbContext.MontlyExpensesDetails.AddRangeAsync(details);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                model.Id = expense.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<MontlyExpensesModel> Update(MontlyExpensesModel model)
        {
            Validate(model);
            if (model.MontlyExpenses_Id == Guid.Empty)
                throw new ArgumentException("Monthly Expenses Id is required.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var expense = await _dbContext.MontlyExpenses
                    .FirstOrDefaultAsync(x =>
                        x.MontlyExpenses_Id == model.MontlyExpenses_Id &&
                        x.DeletedAt == null);

                if (expense == null)
                    throw new KeyNotFoundException("Monthly Expenses not found.");

                expense.MontlyExpenses_Number = model.MontlyExpenses_Number;
                expense.MontlyExpenses = model.MontlyExpenses;
                expense.Company_Id = model.Company_Id;
                expense.StyleId = model.StyleId;
                expense.JobOrder_Id = model.JobOrder_Id;
                expense.Remarks = model.Remarks;
                expense.UpdatedBy = model.UpdatedBy ?? model.CreatedBy;
                expense.UpdatedAt = DateTime.UtcNow;

                var existingDetails = await _dbContext.MontlyExpensesDetails
                    .Where(x => x.MontlyExpenses_Id == model.MontlyExpenses_Id)
                    .ToListAsync();
                var retainedDetailIds = new HashSet<int>();
                var addedDetails = new List<MontlyExpensesDetailDTO>();
                var now = DateTime.UtcNow;

                foreach (var detail in model.MontlyExpenses_Details)
                {
                    var existingDetail = detail.Id > 0
                        ? existingDetails.FirstOrDefault(x => x.Id == detail.Id)
                        : existingDetails.FirstOrDefault(x =>
                            x.MontlyExpenses_Details_Id == detail.MontlyExpenses_Details_Id &&
                            detail.MontlyExpenses_Details_Id != Guid.Empty);

                    if (existingDetail == null)
                    {
                        addedDetails.Add(new MontlyExpensesDetailDTO
                        {
                            MontlyExpenses_Details_Id = detail.MontlyExpenses_Details_Id == Guid.Empty
                                ? Guid.NewGuid()
                                : detail.MontlyExpenses_Details_Id,
                            MontlyExpenses_Id = model.MontlyExpenses_Id,
                            Charge_Id = detail.Charge_Id,
                            AddLessType = detail.AddLessType,
                            Percentage = detail.Percentage,
                            Amount = detail.Amount,
                            Remarks = detail.Remarks,
                            CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                            CreatedAt = now
                        });
                        continue;
                    }

                    if (!retainedDetailIds.Add(existingDetail.Id))
                        throw new ArgumentException("Monthly Expenses details contain a duplicate item.");

                    existingDetail.Charge_Id = detail.Charge_Id;
                    existingDetail.AddLessType = detail.AddLessType;
                    existingDetail.Percentage = detail.Percentage;
                    existingDetail.Amount = detail.Amount;
                    existingDetail.Remarks = detail.Remarks;
                    existingDetail.UpdatedBy = model.UpdatedBy ?? model.CreatedBy;
                    existingDetail.UpdatedAt = now;
                }

                var removedDetails = existingDetails
                    .Where(x => !retainedDetailIds.Contains(x.Id))
                    .ToList();
                _dbContext.MontlyExpensesDetails.RemoveRange(removedDetails);
                await _dbContext.MontlyExpensesDetails.AddRangeAsync(addedDetails);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                model.Id = expense.Id;
                return model;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid montlyExpensesId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var expense = await _dbContext.MontlyExpenses
                    .FirstOrDefaultAsync(x => x.MontlyExpenses_Id == montlyExpensesId);

                if (expense == null)
                    return false;

                var details = await _dbContext.MontlyExpensesDetails
                    .Where(x => x.MontlyExpenses_Id == montlyExpensesId)
                    .ToListAsync();

                _dbContext.MontlyExpensesDetails.RemoveRange(details);
                _dbContext.MontlyExpenses.Remove(expense);
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

        private IQueryable<MontlyExpensesDTO> CreateExpenseQuery()
        {
            return
                from expense in _dbContext.MontlyExpenses
                join company in _dbContext.Company on expense.Company_Id equals company.Id
                join styleEntity in _dbContext.Style on expense.StyleId equals styleEntity.StyleId into styles
                from style in styles.DefaultIfEmpty()
                join jobOrderEntity in _dbContext.JobOrderMaster on expense.JobOrder_Id equals jobOrderEntity.JobOrder_Id into jobOrders
                from jobOrder in jobOrders.DefaultIfEmpty()
                where expense.DeletedAt == null
                select new MontlyExpensesDTO
                {
                    Id = expense.Id,
                    MontlyExpenses_Id = expense.MontlyExpenses_Id,
                    MontlyExpenses_Number = expense.MontlyExpenses_Number,
                    MontlyExpenses = expense.MontlyExpenses,
                    Company_Id = expense.Company_Id,
                    CompanyName = company.Company_Name ?? string.Empty,
                    StyleId = expense.StyleId,
                    StyleName = style == null ? string.Empty : style.Style,
                    JobOrder_Id = expense.JobOrder_Id,
                    JobOrderNumber = jobOrder == null ? string.Empty : jobOrder.JobOrder_Number,
                    Remarks = expense.Remarks,
                    CreatedBy = expense.CreatedBy,
                    CreatedAt = expense.CreatedAt,
                    UpdatedBy = expense.UpdatedBy,
                    UpdatedAt = expense.UpdatedAt,
                    DeletedBy = expense.DeletedBy,
                    DeletedAt = expense.DeletedAt
                };
        }

        private async Task LoadDetails(List<MontlyExpensesDTO> expenses)
        {
            if (expenses.Count == 0)
                return;

            var expenseIds = expenses.Select(x => x.MontlyExpenses_Id).ToList();
            var details = await (
                from detail in _dbContext.MontlyExpensesDetails
                join charge in _dbContext.Charge on detail.Charge_Id equals charge.Charge_Id
                where expenseIds.Contains(detail.MontlyExpenses_Id)
                select new MontlyExpensesDetailDTO
                {
                    Id = detail.Id,
                    MontlyExpenses_Details_Id = detail.MontlyExpenses_Details_Id,
                    MontlyExpenses_Id = detail.MontlyExpenses_Id,
                    Charge_Id = detail.Charge_Id,
                    ChargeName = charge.ChargeName ?? string.Empty,
                    AddLessType = detail.AddLessType,
                    Percentage = detail.Percentage,
                    Amount = detail.Amount,
                    Remarks = detail.Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    DeletedBy = detail.DeletedBy,
                    DeletedAt = detail.DeletedAt
                })
                .AsNoTracking()
                .ToListAsync();

            var detailsByExpense = details
                .GroupBy(x => x.MontlyExpenses_Id)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var expense in expenses)
            {
                if (detailsByExpense.TryGetValue(expense.MontlyExpenses_Id, out var expenseDetails))
                    expense.Details = expenseDetails;
            }
        }

        private static void Validate(MontlyExpensesModel model)
        {
            if (model.Company_Id <= 0)
                throw new ArgumentException("Company Id is required.");
            if (string.IsNullOrWhiteSpace(model.CreatedBy))
                throw new ArgumentException("Created By is required.");
            if (model.MontlyExpenses_Details == null)
                throw new ArgumentException("Monthly Expenses details are required.");

            if (model.MontlyExpenses_Details.Any(x =>
                    x.Charge_Id == Guid.Empty ||
                    (x.AddLessType != "+" && x.AddLessType != "-")))
            {
                throw new ArgumentException("Each detail must have a valid charge and add/less type.");
            }

            if (model.MontlyExpenses_Details
                .GroupBy(x => x.Charge_Id)
                .Any(x => x.Count() > 1))
            {
                throw new ArgumentException("A charge can only be added once to Monthly Expenses.");
            }

            if (model.MontlyExpenses_Details
                .Where(x => x.MontlyExpenses_Details_Id != Guid.Empty)
                .GroupBy(x => x.MontlyExpenses_Details_Id)
                .Any(x => x.Count() > 1))
            {
                throw new ArgumentException("Monthly Expenses details contain a duplicate identifier.");
            }
        }
    }
}
