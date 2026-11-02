using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Production
{
    public class IssueRepository : IIssueRepository
    {
        private readonly AppDBContext _dbContext;

        public IssueRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastIssueNumber = await _dbContext.IssueMaster
                .OrderByDescending(issue => issue.Issue_Number)
                .Select(issue => issue.Issue_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.Issue,
                Utility.CommonFunctions.GetTransactionNumber(lastIssueNumber));
        }

        public async Task<List<IssueMasterDTO>> GetIssues()
        {
            return await (
                from issue in _dbContext.IssueMaster.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking() on issue.Company_Id equals (int?)company.Id into companies
                from company in companies.DefaultIfEmpty()
                join supplier in _dbContext.Supplier.AsNoTracking() on issue.Supplier_Id equals (Guid?)supplier.Supplier_Id into suppliers
                from supplier in suppliers.DefaultIfEmpty()
                where !issue.IsDeleted
                orderby issue.Id descending
                select new IssueMasterDTO
                {
                    Id = issue.Id,
                    Issue_Id = issue.Issue_Id,
                    Issue_Number = issue.Issue_Number,
                    Issue_Date = issue.Issue_Date,
                    Supplier_Id = issue.Supplier_Id,
                    Issue_Type = issue.Issue_Type,
                    Joborderno = issue.Joborderno,
                    StyleId = issue.StyleId,
                    Unit_supplier_self = issue.Unit_supplier_self,
                    GatePassVehicle = issue.GatePassVehicle,
                    Remarks = issue.Remarks,
                    Terms_Conditions = issue.Terms_Conditions,
                    Company_Id = issue.Company_Id,
                    CreatedBy = issue.CreatedBy,
                    CreatedAt = issue.CreatedAt,
                    UpdatedBy = issue.UpdatedBy,
                    UpdatedAt = issue.UpdatedAt,
                    IsActive = issue.IsActive,
                    IsDeleted = issue.IsDeleted,
                    Company_Name = company != null ? company.Company_Name : null,
                    Supplier_Name = supplier != null ? supplier.Supplier_Name : null
                }).ToListAsync();
        }

        public async Task<IssueMasterDTO> GetIssue(Guid issueId)
        {
            var issue = await (
                from master in _dbContext.IssueMaster.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking() on master.Company_Id equals (int?)company.Id into companies
                from company in companies.DefaultIfEmpty()
                join supplier in _dbContext.Supplier.AsNoTracking() on master.Supplier_Id equals (Guid?)supplier.Supplier_Id into suppliers
                from supplier in suppliers.DefaultIfEmpty()
                where master.Issue_Id == issueId && !master.IsDeleted
                select new IssueMasterDTO
                {
                    Id = master.Id,
                    Issue_Id = master.Issue_Id,
                    Issue_Number = master.Issue_Number,
                    Issue_Date = master.Issue_Date,
                    Supplier_Id = master.Supplier_Id,
                    Issue_Type = master.Issue_Type,
                    Joborderno = master.Joborderno,
                    StyleId = master.StyleId,
                    Unit_supplier_self = master.Unit_supplier_self,
                    GatePassVehicle = master.GatePassVehicle,
                    Remarks = master.Remarks,
                    Terms_Conditions = master.Terms_Conditions,
                    Company_Id = master.Company_Id,
                    CreatedBy = master.CreatedBy,
                    CreatedAt = master.CreatedAt,
                    UpdatedBy = master.UpdatedBy,
                    UpdatedAt = master.UpdatedAt,
                    IsActive = master.IsActive,
                    IsDeleted = master.IsDeleted,
                    Company_Name = company != null ? company.Company_Name : null,
                    Supplier_Name = supplier != null ? supplier.Supplier_Name : null
                }).FirstOrDefaultAsync();

            if (issue == null)
            {
                return new IssueMasterDTO();
            }

            issue.Details = await (
                from detail in _dbContext.IssueDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on detail.Item_Id equals item.Item_Id
                join color in _dbContext.Color.AsNoTracking() on detail.Color_Id equals (Guid?)color.Color_Id into colors
                from color in colors.DefaultIfEmpty()
                join size in _dbContext.Size.AsNoTracking() on detail.Size_Id equals (Guid?)size.Size_Id into sizes
                from size in sizes.DefaultIfEmpty()
                where detail.Issue_Id == issueId
                orderby detail.Id
                select new IssueDetailDTO
                {
                    Id = detail.Id,
                    Issue_Details_Id = detail.Issue_Details_Id,
                    Issue_Id = detail.Issue_Id,
                    Item_Id = detail.Item_Id,
                    Size_Id = detail.Size_Id,
                    Color_Id = detail.Color_Id,
                    UOM_Id = detail.UOM_Id,
                    Quantity = detail.Quantity,
                    Rate = detail.Rate,
                    Remarks = detail.Remarks,
                    CreatedBy = detail.CreatedBy,
                    CreatedAt = detail.CreatedAt,
                    UpdatedBy = detail.UpdatedBy,
                    UpdatedAt = detail.UpdatedAt,
                    Item_Name = item.Item_Name,
                    Color_Name = color != null ? color.Color_Name : null,
                    Size_Name = size != null ? size.Size_Name : null
                }).ToListAsync();

            var detailIds = issue.Details.Select(detail => detail.Issue_Details_Id).ToList();
            issue.Stock_Allocations = await _dbContext.IssueStockAllocations
                .AsNoTracking()
                .Where(allocation => detailIds.Contains(allocation.Issue_Details_Id))
                .OrderBy(allocation => allocation.Id)
                .ToListAsync();

            return issue;
        }

        public async Task<IssueMasterDTO> Save(IssueModel model)
        {
            Validate(model);

            // An edit posted as a create must update the existing issue, not insert a duplicate.
            if (model.Issue_Id != Guid.Empty
                && await _dbContext.IssueMaster.AnyAsync(x => x.Issue_Id == model.Issue_Id && !x.IsDeleted))
            {
                return await Update(model);
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (await _dbContext.IssueMaster.AnyAsync(x => x.Issue_Number == model.Issue_Number))
                {
                    throw new ArgumentException($"Issue number {model.Issue_Number} already exists.");
                }

                var now = DateTime.UtcNow;
                var createdBy = model.CreatedBy!;
                var issueId = model.Issue_Id == Guid.Empty ? Guid.NewGuid() : model.Issue_Id;

                var issue = new IssueMasterDTO
                {
                    Issue_Id = issueId,
                    Issue_Number = model.Issue_Number,
                    Issue_Date = model.Issue_Date,
                    Supplier_Id = model.Supplier_Id,
                    Issue_Type = model.Issue_Type,
                    Joborderno = model.Joborderno,
                    StyleId = model.StyleId,
                    Unit_supplier_self = model.Unit_supplier_self,
                    GatePassVehicle = model.GatePassVehicle,
                    Remarks = model.Remarks,
                    Terms_Conditions = model.Terms_Conditions,
                    Company_Id = model.Company_Id,
                    CreatedBy = createdBy,
                    CreatedAt = now,
                    IsActive = model.IsActive,
                    IsDeleted = false
                };

                await _dbContext.IssueMaster.AddAsync(issue);
                await _dbContext.SaveChangesAsync();

                var details = model.Issue_Details
                    .Select(detail => CreateDetail(detail, issueId, createdBy, now))
                    .ToList();

                if (details.Count > 0)
                {
                    await _dbContext.IssueDetails.AddRangeAsync(details);
                    await _dbContext.SaveChangesAsync();
                }

                var allocations = BuildAllocations(model, details.Select(d => d.Issue_Details_Id).ToHashSet(), createdBy, now);
                if (allocations.Count > 0)
                {
                    await _dbContext.IssueStockAllocations.AddRangeAsync(allocations);
                    await ApplyStock(allocations, 1, updatedBy: createdBy, now);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetIssue(issueId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IssueMasterDTO> Update(IssueModel model)
        {
            if (model.Issue_Id == Guid.Empty)
            {
                throw new ArgumentException("Issue Id is required.");
            }

            Validate(model);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var issue = await _dbContext.IssueMaster.FirstOrDefaultAsync(x =>
                    x.Issue_Id == model.Issue_Id && !x.IsDeleted);

                if (issue == null)
                {
                    throw new KeyNotFoundException("Issue not found.");
                }

                if (await _dbContext.IssueMaster.AnyAsync(x =>
                        x.Issue_Number == model.Issue_Number && x.Issue_Id != model.Issue_Id))
                {
                    throw new ArgumentException($"Issue number {model.Issue_Number} already exists.");
                }

                var now = DateTime.UtcNow;
                var updatedBy = model.UpdatedBy ?? model.CreatedBy!;

                issue.Issue_Number = model.Issue_Number;
                issue.Issue_Date = model.Issue_Date;
                issue.Supplier_Id = model.Supplier_Id;
                issue.Issue_Type = model.Issue_Type;
                issue.Joborderno = model.Joborderno;
                issue.StyleId = model.StyleId;
                issue.Unit_supplier_self = model.Unit_supplier_self;
                issue.GatePassVehicle = model.GatePassVehicle;
                issue.Remarks = model.Remarks;
                issue.Terms_Conditions = model.Terms_Conditions;
                issue.Company_Id = model.Company_Id;
                issue.IsActive = model.IsActive;
                issue.UpdatedBy = updatedBy;
                issue.UpdatedAt = now;

                var existingDetails = await _dbContext.IssueDetails
                    .Where(detail => detail.Issue_Id == model.Issue_Id)
                    .ToListAsync();
                var existingIds = existingDetails.Select(detail => detail.Issue_Details_Id).ToList();

                // Allocations are replaced wholesale; they must go before details because of the FK.
                var existingAllocations = await _dbContext.IssueStockAllocations
                    .Where(allocation => existingIds.Contains(allocation.Issue_Details_Id))
                    .ToListAsync();
                await ApplyStock(existingAllocations, -1, updatedBy, now);
                _dbContext.IssueStockAllocations.RemoveRange(existingAllocations);
                await _dbContext.SaveChangesAsync();

                var retainedIds = model.Issue_Details
                    .Where(detail => existingIds.Contains(detail.Issue_Details_Id))
                    .Select(detail => detail.Issue_Details_Id)
                    .ToHashSet();

                _dbContext.IssueDetails.RemoveRange(
                    existingDetails.Where(detail => !retainedIds.Contains(detail.Issue_Details_Id)));

                var finalDetailIds = new HashSet<Guid>();
                foreach (var detail in model.Issue_Details)
                {
                    var existing = existingDetails.FirstOrDefault(x => x.Issue_Details_Id == detail.Issue_Details_Id);

                    if (existing == null)
                    {
                        var created = CreateDetail(detail, model.Issue_Id, updatedBy, now);
                        finalDetailIds.Add(created.Issue_Details_Id);
                        await _dbContext.IssueDetails.AddAsync(created);
                        continue;
                    }

                    finalDetailIds.Add(existing.Issue_Details_Id);
                    existing.Item_Id = detail.Item_Id;
                    existing.Size_Id = detail.Size_Id;
                    existing.Color_Id = detail.Color_Id;
                    existing.UOM_Id = detail.UOM_Id;
                    existing.Quantity = detail.Quantity;
                    existing.Rate = detail.Rate;
                    existing.Remarks = detail.Remarks;
                    existing.UpdatedBy = updatedBy;
                    existing.UpdatedAt = now;
                }

                await _dbContext.SaveChangesAsync();

                var allocations = BuildAllocations(model, finalDetailIds, updatedBy, now);
                if (allocations.Count > 0)
                {
                    await _dbContext.IssueStockAllocations.AddRangeAsync(allocations);
                    await ApplyStock(allocations, 1, updatedBy, now);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return await GetIssue(model.Issue_Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid issueId, string? deletedBy)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var issue = await _dbContext.IssueMaster
                    .FirstOrDefaultAsync(x => x.Issue_Id == issueId && !x.IsDeleted);

                if (issue == null)
                {
                    return false;
                }

                var now = DateTime.UtcNow;
                var detailIds = await _dbContext.IssueDetails
                    .Where(d => d.Issue_Id == issueId)
                    .Select(d => d.Issue_Details_Id)
                    .ToListAsync();

                // Return the issued quantity to stock and drop the allocations.
                var allocations = await _dbContext.IssueStockAllocations
                    .Where(a => detailIds.Contains(a.Issue_Details_Id))
                    .ToListAsync();
                await ApplyStock(allocations, -1, deletedBy, now);
                _dbContext.IssueStockAllocations.RemoveRange(allocations);

                issue.IsDeleted = true;
                issue.IsActive = false;
                issue.UpdatedBy = deletedBy;
                issue.UpdatedAt = now;

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

        // direction +1 issues stock (AllotedQty up, BalQty down); -1 reverses it.
        private async Task ApplyStock(
            IEnumerable<IssueStockAllocationDTO> allocations,
            int direction,
            string? updatedBy,
            DateTime now)
        {
            var totals = allocations
                .GroupBy(a => a.Stock_Id)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Allocated_Qty));

            if (totals.Count == 0)
            {
                return;
            }

            var stockIds = totals.Keys.ToList();
            var stocks = await _dbContext.StockDetails
                .Where(s => stockIds.Contains(s.Stock_Id) && s.DeletedAt == null)
                .ToListAsync();

            foreach (var (stockId, qty) in totals)
            {
                var stock = stocks.FirstOrDefault(s => s.Stock_Id == stockId)
                    ?? throw new ArgumentException($"Stock {stockId} not found.");

                var delta = qty * direction;
                var balance = (stock.BalQty ?? stock.Qty) - delta;

                if (balance < 0)
                {
                    throw new ArgumentException(
                        $"Insufficient balance for stock {stock.StockNo}. Available {stock.BalQty ?? stock.Qty}, requested {qty}.");
                }

                stock.AllotedQty = Math.Max(0, (stock.AllotedQty ?? 0) + delta);
                stock.BalQty = balance;
                stock.UpdatedBy = updatedBy;
                stock.UpdatedAt = now;
            }
        }

        private static void Validate(IssueModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CreatedBy) && string.IsNullOrWhiteSpace(model.UpdatedBy))
            {
                throw new ArgumentException("CreatedBy or UpdatedBy is required.");
            }

            if (model.Unit_supplier_self != "S" && model.Unit_supplier_self != "U")
            {
                throw new ArgumentException("Unit_supplier_self must be 'S' or 'U'.");
            }

            if (model.Issue_Details.Any(detail => detail.Quantity <= 0 || detail.Rate < 0))
            {
                throw new ArgumentException("Detail quantity must be greater than zero and rate cannot be negative.");
            }

            if (model.Issue_Stock_Allocations.Any(allocation => allocation.Allocated_Qty <= 0))
            {
                throw new ArgumentException("Allocated quantity must be greater than zero.");
            }
        }

        private static IssueDetailDTO CreateDetail(
            IssueDetailDTO detail,
            Guid issueId,
            string createdBy,
            DateTime createdAt)
        {
            return new IssueDetailDTO
            {
                // A client-supplied id lets new stock allocations reference this detail in the same request.
                Issue_Details_Id = detail.Issue_Details_Id == Guid.Empty ? Guid.NewGuid() : detail.Issue_Details_Id,
                Issue_Id = issueId,
                Item_Id = detail.Item_Id,
                Size_Id = detail.Size_Id,
                Color_Id = detail.Color_Id,
                UOM_Id = detail.UOM_Id,
                Quantity = detail.Quantity,
                Rate = detail.Rate,
                Remarks = detail.Remarks,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            };
        }

        private static List<IssueStockAllocationDTO> BuildAllocations(
            IssueModel model,
            HashSet<Guid> detailIds,
            string createdBy,
            DateTime createdAt)
        {
            var allocations = new List<IssueStockAllocationDTO>();

            foreach (var allocation in model.Issue_Stock_Allocations)
            {
                if (!detailIds.Contains(allocation.Issue_Details_Id))
                {
                    throw new ArgumentException(
                        $"Stock allocation references unknown issue detail {allocation.Issue_Details_Id}.");
                }

                allocations.Add(new IssueStockAllocationDTO
                {
                    Issue_Stock_Allocation_Id = Guid.NewGuid(),
                    Issue_Details_Id = allocation.Issue_Details_Id,
                    Stock_Id = allocation.Stock_Id,
                    Allocated_Qty = allocation.Allocated_Qty,
                    CreatedBy = createdBy,
                    CreatedAt = createdAt
                });
            }

            return allocations;
        }
    }
}
