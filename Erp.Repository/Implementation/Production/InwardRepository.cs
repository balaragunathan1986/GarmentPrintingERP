using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Implementation.Production
{
    public class InwardRepository : IInwardRepository
    {
        private readonly AppDBContext _dbContext;
        public InwardRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.InwardMaster
                .OrderByDescending(x => x.Inward_Number)
                .Select(x => x.Inward_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.Inward, Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }
        public async Task<Guid> CreateOrUpdate(InwardViewModel inward)
        {
            if (inward.Inward_master == null)
                throw new ArgumentException("Inward master is required.", nameof(inward));

            var master = inward.Inward_master;
            var incomingItems = inward.Inward_Items?.ToList() ?? new List<InwardItemDTO>();
            var incomingStocks = inward.Inward_Stocks?.Where(stock => stock.Quantity > 0).ToList() ?? new List<InwardStockDTO>();
            var existingInward = await _dbContext.InwardMaster.FindAsync(master.Id);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            if (existingInward == null)
            {
                if (master.Inward_Id == Guid.Empty)
                {
                    master.Inward_Id = Guid.NewGuid();
                }

                await _dbContext.InwardMaster.AddAsync(master);
            }
            else
            {
                var existingStocks = await _dbContext.InwardStocks
                    .Where(stock => stock.Inward_Id == existingInward.Inward_Id)
                    .ToListAsync();
                await RestoreStockBalances(existingStocks);
                _dbContext.InwardStocks.RemoveRange(existingStocks);
                await _dbContext.SaveChangesAsync();

                existingInward.Inward_Number = master.Inward_Number;
                existingInward.Inward_Date = master.Inward_Date;
                existingInward.JobOrder_Number = master.JobOrder_Number;
                existingInward.Supplier_Id = master.Supplier_Id;
                existingInward.Supplier_Dc_Number = master.Supplier_Dc_Number;
                existingInward.Remarks = master.Remarks;
                master.Inward_Id = existingInward.Inward_Id;

                var existingItems = await _dbContext.InwardItems
                    .Where(item => item.Inward_Id == existingInward.Inward_Id)
                    .ToListAsync();
                _dbContext.InwardItems.RemoveRange(existingItems);
            }

            foreach (var item in incomingItems)
            {
                item.Id = 0;
                if (item.Inward_Item_Id == Guid.Empty)
                {
                    item.Inward_Item_Id = Guid.NewGuid();
                }
                item.Inward_Id = master.Inward_Id;
            }

            await _dbContext.InwardItems.AddRangeAsync(incomingItems);
            await _dbContext.SaveChangesAsync();

            await ApplyStockUtilization(master.Inward_Id, incomingItems, incomingStocks);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return master.Inward_Id;
        }

        public async Task<bool> DeleteInward(Guid Inward_Id)
        {
            var existingInward = await _dbContext.InwardMaster.FirstOrDefaultAsync(x=>x.Inward_Id == Inward_Id);
            if (existingInward == null)
                return true;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            var existingStocks = await _dbContext.InwardStocks
                .Where(stock => stock.Inward_Id == existingInward.Inward_Id)
                .ToListAsync();
            await RestoreStockBalances(existingStocks);
            _dbContext.InwardStocks.RemoveRange(existingStocks);
            await _dbContext.SaveChangesAsync();

            var existingItems = await _dbContext.InwardItems
                .Where(item => item.Inward_Id == existingInward.Inward_Id)
                .ToListAsync();
            _dbContext.InwardItems.RemoveRange(existingItems);
            await _dbContext.SaveChangesAsync();

            _dbContext.InwardMaster.Remove(existingInward);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<InwardViewModel> GetInward(Guid Inward_Id)
        {
            var master = await _dbContext.InwardMaster.FirstOrDefaultAsync(x => x.Inward_Id == Inward_Id);
            var items = await _dbContext.InwardItems.Where(x => x.Inward_Id == Inward_Id).ToListAsync();
            var stocks = await GetInwardStocks(Inward_Id);

            return new InwardViewModel
            {
                Inward_master = master,
                Inward_Items = items,
                Inward_Stocks = stocks
            };
        }

        public async Task<IEnumerable<InwardStockDTO>> GetAvailableStockDetails()
        {
            return await (
                from stock in _dbContext.StockDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on stock.ItemId equals (Guid?)item.Item_Id into items
                from item in items.DefaultIfEmpty()
                join color in _dbContext.Color.AsNoTracking() on stock.ColorId equals (Guid?)color.Color_Id into colors
                from color in colors.DefaultIfEmpty()
                join size in _dbContext.Size.AsNoTracking() on stock.SizeId equals (Guid?)size.Size_Id into sizes
                from size in sizes.DefaultIfEmpty()
                join uom in _dbContext.Uom.AsNoTracking() on stock.Uom_Id equals (Guid?)uom.Uom_Id into uoms
                from uom in uoms.DefaultIfEmpty()
                where stock.IsActive && !stock.IsDeleted && (stock.BalQty ?? stock.Qty) > 0
                orderby stock.StockNo
                select new InwardStockDTO
                {
                    Stock_Id = stock.Stock_Id,
                    Stock_Name = stock.StockNo,
                    Item_Name = item == null ? null : item.Item_Name,
                    Color_Name = color == null ? null : color.Color_Name,
                    Size_Name = size == null ? null : size.Size_Name,
                    BalQty = stock.BalQty ?? stock.Qty,
                    Uom_Id = stock.Uom_Id,
                    Uom_Name = uom == null ? null : uom.Uom
                }).ToListAsync();
        }

        public async Task<IEnumerable<InwardStockDTO>> GetStockReport()
        {
            return await (
                from stock in _dbContext.StockDetails.AsNoTracking()
                join item in _dbContext.Item.AsNoTracking() on stock.ItemId equals (Guid?)item.Item_Id into items
                from item in items.DefaultIfEmpty()
                join color in _dbContext.Color.AsNoTracking() on stock.ColorId equals (Guid?)color.Color_Id into colors
                from color in colors.DefaultIfEmpty()
                join size in _dbContext.Size.AsNoTracking() on stock.SizeId equals (Guid?)size.Size_Id into sizes
                from size in sizes.DefaultIfEmpty()
                join uom in _dbContext.Uom.AsNoTracking() on stock.Uom_Id equals (Guid?)uom.Uom_Id into uoms
                from uom in uoms.DefaultIfEmpty()
                where stock.IsActive && !stock.IsDeleted
                orderby stock.StockNo
                select new InwardStockDTO
                {
                    Stock_Id = stock.Stock_Id,
                    Stock_Name = stock.StockNo,
                    Item_Name = item == null ? null : item.Item_Name,
                    Color_Name = color == null ? null : color.Color_Name,
                    Size_Name = size == null ? null : size.Size_Name,
                    Qty = stock.Qty,
                    Quantity = stock.Qty - (stock.BalQty ?? stock.Qty),
                    BalQty = stock.BalQty ?? stock.Qty,
                    Uom_Id = stock.Uom_Id,
                    Uom_Name = uom == null ? null : uom.Uom
                }).ToListAsync();
        }

        private async Task ApplyStockUtilization(
            Guid inwardId,
            IReadOnlyCollection<InwardItemDTO> inwardItems,
            IReadOnlyCollection<InwardStockDTO> allocations)
        {
            if (allocations.Count == 0)
                return;

            var inwardItemIds = inwardItems.Select(item => item.Inward_Item_Id).ToHashSet();
            if (allocations.Any(allocation => !inwardItemIds.Contains(allocation.Inward_Item_Id)))
                throw new InvalidOperationException("Each stock utilization entry must reference an inward item.");

            var allocationsByStock = allocations.GroupBy(allocation => allocation.Stock_Id).ToList();
            var stockIds = allocationsByStock.Select(group => group.Key).ToList();
            var stockById = await _dbContext.StockDetails
                .Where(stock => stockIds.Contains(stock.Stock_Id) && stock.IsActive && !stock.IsDeleted)
                .ToDictionaryAsync(stock => stock.Stock_Id);

            if (stockById.Count != stockIds.Count)
                throw new InvalidOperationException("One or more selected stock details are unavailable.");

            foreach (var stockAllocation in allocationsByStock)
            {
                var stock = stockById[stockAllocation.Key];
                var utilizedQuantity = stockAllocation.Sum(allocation => allocation.Quantity);
                var availableQuantity = stock.BalQty ?? stock.Qty;
                if (utilizedQuantity > availableQuantity)
                    throw new InvalidOperationException($"Stock {stock.StockNo} has insufficient balance quantity.");

                stock.BalQty = availableQuantity - utilizedQuantity;
                foreach (var allocation in stockAllocation)
                {
                    allocation.Id = 0;
                    allocation.Inward_Stock_Id = allocation.Inward_Stock_Id == Guid.Empty
                        ? Guid.NewGuid()
                        : allocation.Inward_Stock_Id;
                    allocation.Inward_Id = inwardId;
                    allocation.Uom_Id ??= stock.Uom_Id;
                    allocation.CreatedAt ??= DateTime.UtcNow;
                    allocation.CreatedBy ??= "system";
                }
            }

            await _dbContext.InwardStocks.AddRangeAsync(allocations);
        }

        private async Task RestoreStockBalances(IEnumerable<InwardStockDTO> allocations)
        {
            var allocationList = allocations.ToList();
            if (allocationList.Count == 0)
                return;

            var stockIds = allocationList.Select(allocation => allocation.Stock_Id).Distinct().ToList();
            var stockById = await _dbContext.StockDetails
                .Where(stock => stockIds.Contains(stock.Stock_Id))
                .ToDictionaryAsync(stock => stock.Stock_Id);

            foreach (var stockAllocation in allocationList.GroupBy(allocation => allocation.Stock_Id))
            {
                if (stockById.TryGetValue(stockAllocation.Key, out var stock))
                {
                    stock.BalQty = (stock.BalQty ?? stock.Qty) + stockAllocation.Sum(allocation => allocation.Quantity);
                }
            }
        }

        private async Task<List<InwardStockDTO>> GetInwardStocks(Guid inwardId)
        {
            return await (
                from allocation in _dbContext.InwardStocks.AsNoTracking()
                join stock in _dbContext.StockDetails.AsNoTracking() on allocation.Stock_Id equals stock.Stock_Id
                join item in _dbContext.Item.AsNoTracking() on stock.ItemId equals (Guid?)item.Item_Id into items
                from item in items.DefaultIfEmpty()
                join color in _dbContext.Color.AsNoTracking() on stock.ColorId equals (Guid?)color.Color_Id into colors
                from color in colors.DefaultIfEmpty()
                join size in _dbContext.Size.AsNoTracking() on stock.SizeId equals (Guid?)size.Size_Id into sizes
                from size in sizes.DefaultIfEmpty()
                join uom in _dbContext.Uom.AsNoTracking() on allocation.Uom_Id equals (Guid?)uom.Uom_Id into uoms
                from uom in uoms.DefaultIfEmpty()
                where allocation.Inward_Id == inwardId
                select new InwardStockDTO
                {
                    Id = allocation.Id,
                    Inward_Stock_Id = allocation.Inward_Stock_Id,
                    Inward_Item_Id = allocation.Inward_Item_Id,
                    Inward_Id = allocation.Inward_Id,
                    Stock_Id = allocation.Stock_Id,
                    Quantity = allocation.Quantity,
                    Uom_Id = allocation.Uom_Id,
                    Remarks = allocation.Remarks,
                    Stock_Name = stock.StockNo,
                    Item_Name = item == null ? null : item.Item_Name,
                    Color_Name = color == null ? null : color.Color_Name,
                    Size_Name = size == null ? null : size.Size_Name,
                    BalQty = (stock.BalQty ?? stock.Qty) + allocation.Quantity,
                    Uom_Name = uom == null ? null : uom.Uom
                }).ToListAsync();
        }

        public async Task<IEnumerable<object>> GetInward()
        {
           return await _dbContext.InwardMaster.Join(_dbContext.Supplier,
                    inward => inward.Supplier_Id,
                    supplier => supplier.Supplier_Id,
                    (inward, supplier) => new
                    {
                        Id = inward.Id,
                        Inward_Id = inward.Inward_Id,
                        Inward_Number = inward.Inward_Number,
                        Inward_Date = inward.Inward_Date,
                        JobOrder_Number = inward.JobOrder_Number,
                        Supplier_Id = inward.Supplier_Id,
                        Supplier_Dc_Number = inward.Supplier_Dc_Number,
                        Remarks = inward.Remarks,
                        Supplier_Name = supplier.Supplier_Name
                    })
                    .OrderByDescending(x => x.Inward_Number)
                    .ToListAsync();
        }
    }
}
