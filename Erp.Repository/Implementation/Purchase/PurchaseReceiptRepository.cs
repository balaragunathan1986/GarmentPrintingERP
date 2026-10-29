using Erp.Domain.DTO;
using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model.Purchase;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Purchase
{
    public class PurchaseReceiptRepository : IPurchaseReceiptRepository
    {
        private readonly AppDBContext _dbContext;

        public PurchaseReceiptRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.PurchaseReceiptMaster
                .OrderByDescending(receipt => receipt.Receipt_No)
                .Select(receipt => receipt.Receipt_No)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.PurchaseReceipt,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public async Task<List<PurchaseReceiptMasterDTO>> GetReceipts(
            string? searchText,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query =
                from receipt in _dbContext.PurchaseReceiptMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on receipt.Supplier_Id equals supplier.Supplier_Id
                join company in _dbContext.Company.AsNoTracking()
                    on receipt.Company_Id equals company.Company_Id
                where receipt.DeletedAt == null
                    && (string.IsNullOrWhiteSpace(searchText)
                        || receipt.Receipt_No.Contains(searchText)
                        || (receipt.Supplier_Invoice_No ?? string.Empty).Contains(searchText)
                        || (supplier.Supplier_Name ?? string.Empty).Contains(searchText))
                    && (!fromDate.HasValue || receipt.Receipt_Date >= fromDate.Value)
                    && (!toDate.HasValue || receipt.Receipt_Date < toDate.Value.Date.AddDays(1))
                orderby receipt.Id descending
                select new PurchaseReceiptMasterDTO
                {
                    Id = receipt.Id,
                    PurchaseReceipt_Id = receipt.PurchaseReceipt_Id,
                    Receipt_No = receipt.Receipt_No,
                    Receipt_Date = receipt.Receipt_Date,
                    Company_Id = receipt.Company_Id,
                    Company_Name = company.Company_Name,
                    Supplier_Id = receipt.Supplier_Id,
                    Supplier_Name = supplier.Supplier_Name,
                    Dc_No = receipt.Dc_No,
                    Dc_Date = receipt.Dc_Date,
                    Supplier_Invoice_No = receipt.Supplier_Invoice_No,
                    Supplier_Invoice_Date = receipt.Supplier_Invoice_Date,
                    Remarks = receipt.Remarks,
                    IsApproved = receipt.IsApproved,
                    IsCancelled = receipt.IsCancelled,
                    CreatedBy = receipt.CreatedBy,
                    CreatedAt = receipt.CreatedAt,
                    UpdatedBy = receipt.UpdatedBy,
                    UpdatedAt = receipt.UpdatedAt,
                    DeletedBy = receipt.DeletedBy,
                    DeletedAt = receipt.DeletedAt
                };

            return await query.ToListAsync();
        }

        public async Task<PurchaseReceiptViewModel?> GetReceipt(Guid receiptId)
        {
            var master = await (
                from receipt in _dbContext.PurchaseReceiptMaster.AsNoTracking()
                join supplier in _dbContext.Supplier.AsNoTracking()
                    on receipt.Supplier_Id equals supplier.Supplier_Id
                join company in _dbContext.Company.AsNoTracking()
                    on receipt.Company_Id equals company.Company_Id
                where receipt.PurchaseReceipt_Id == receiptId && receipt.DeletedAt == null
                select new PurchaseReceiptMasterDTO
                {
                    Id = receipt.Id,
                    PurchaseReceipt_Id = receipt.PurchaseReceipt_Id,
                    Receipt_No = receipt.Receipt_No,
                    Receipt_Date = receipt.Receipt_Date,
                    Company_Id = receipt.Company_Id,
                    Supplier_Id = receipt.Supplier_Id,
                    Dc_No = receipt.Dc_No,
                    Dc_Date = receipt.Dc_Date,
                    Supplier_Invoice_No = receipt.Supplier_Invoice_No,
                    Supplier_Invoice_Date = receipt.Supplier_Invoice_Date,
                    Remarks = receipt.Remarks,
                    IsApproved = receipt.IsApproved,
                    IsCancelled = receipt.IsCancelled,
                    CreatedBy = receipt.CreatedBy,
                    CreatedAt = receipt.CreatedAt,
                    UpdatedBy = receipt.UpdatedBy,
                    UpdatedAt = receipt.UpdatedAt,
                    DeletedBy = receipt.DeletedBy,
                    DeletedAt = receipt.DeletedAt,
                    Supplier_Name = supplier.Supplier_Name,
                    Company_Name = company.Company_Name
                }).FirstOrDefaultAsync();

            if (master == null)
            {
                return null;
            }

            var details = await (
                from receiptDetail in _dbContext.PurchaseReceiptDetails.AsNoTracking()
                join purchaseDetail in _dbContext.PurchaseDetails.AsNoTracking()
                    on receiptDetail.Purchase_Detail_Id equals purchaseDetail.Purchase_Detail_Id
                join purchase in _dbContext.PurchaseMaster.AsNoTracking()
                    on purchaseDetail.Purchase_Id equals purchase.Purchase_Id
                join item in _dbContext.Item.AsNoTracking()
                    on purchaseDetail.Item_Id equals item.Item_Id
                where receiptDetail.PurchaseReceipt_Id == receiptId
                    && receiptDetail.DeletedAt == null
                orderby receiptDetail.Id
                select new PurchaseReceiptDetailDTO
                {
                    Id = receiptDetail.Id,
                    PurchaseReceipt_Detail_Id = receiptDetail.PurchaseReceipt_Detail_Id,
                    PurchaseReceipt_Id = receiptDetail.PurchaseReceipt_Id,
                    Purchase_Detail_Id = receiptDetail.Purchase_Detail_Id,
                    Received_Qty = receiptDetail.Received_Qty,
                    Accepted_Qty = receiptDetail.Accepted_Qty,
                    Rejected_Qty = receiptDetail.Rejected_Qty,
                    Shortage_Qty = receiptDetail.Shortage_Qty,
                    Rate = receiptDetail.Rate,
                    Remarks = receiptDetail.Remarks,
                    CreatedBy = receiptDetail.CreatedBy,
                    CreatedAt = receiptDetail.CreatedAt,
                    UpdatedBy = receiptDetail.UpdatedBy,
                    UpdatedAt = receiptDetail.UpdatedAt,
                    DeletedBy = receiptDetail.DeletedBy,
                    DeletedAt = receiptDetail.DeletedAt,
                    Purchase_Id = purchase.Purchase_Id,
                    Purchase_No = purchase.Purchase_No,
                    Item_Id = purchaseDetail.Item_Id,
                    Item_Name = item.Item_Name,
                    UOM_Id = purchaseDetail.UOM_Id,
                    Color_Id = purchaseDetail.Color_Id,
                    Size_Id = purchaseDetail.Size_Id,
                    Ordered_Qty = purchaseDetail.Quantity,
                    Previously_Received_Qty = purchaseDetail.Received_Qty
                        - receiptDetail.Received_Qty
                }).ToListAsync();

            return new PurchaseReceiptViewModel
            {
                PurchaseReceipt_Master = master,
                PurchaseReceipt_Details = details
            };
        }

        public async Task<List<PurchaseReceiptDetailDTO>> GetPurchaseDetails(Guid purchaseId)
        {
            return await (
                from purchase in _dbContext.PurchaseMaster.AsNoTracking()
                join purchaseDetail in _dbContext.PurchaseDetails.AsNoTracking()
                    on purchase.Purchase_Id equals purchaseDetail.Purchase_Id
                join item in _dbContext.Item.AsNoTracking()
                    on purchaseDetail.Item_Id equals item.Item_Id
                where purchase.Purchase_Id == purchaseId
                    && purchase.DeletedAt == null
                    && purchaseDetail.DeletedAt == null
                orderby purchaseDetail.Id
                select new PurchaseReceiptDetailDTO
                {
                    Purchase_Id = purchase.Purchase_Id,
                    Purchase_Detail_Id = purchaseDetail.Purchase_Detail_Id,
                    Purchase_No = purchase.Purchase_No,
                    Item_Id = purchaseDetail.Item_Id,
                    Item_Name = item.Item_Name,
                    UOM_Id = purchaseDetail.UOM_Id,
                    Color_Id = purchaseDetail.Color_Id,
                    Size_Id = purchaseDetail.Size_Id,
                    Ordered_Qty = purchaseDetail.Quantity,
                    Previously_Received_Qty = purchaseDetail.Received_Qty,
                    Rate = purchaseDetail.Rate
                }).ToListAsync();
        }

        public async Task<PurchaseReceiptViewModel> Save(PurchaseReceiptViewModel model)
        {
            var source = model.PurchaseReceipt_Master
                ?? throw new ArgumentException("PurchaseReceipt_Master is required.");
            var sourceDetails = model.PurchaseReceipt_Details
                ?? throw new ArgumentException("PurchaseReceipt_Details is required.");
            if (sourceDetails.Count == 0)
            {
                throw new ArgumentException("At least one purchase receipt detail is required.");
            }

            if (string.IsNullOrWhiteSpace(source.Receipt_No))
            {
                throw new ArgumentException("Receipt_No is required.");
            }

            if (source.Receipt_No.Length > 15)
            {
                throw new ArgumentException("Receipt_No cannot exceed 15 characters when adding stock.");
            }

            var purchaseDetailIds = sourceDetails
                .Select(detail => detail.Purchase_Detail_Id)
                .ToList();
            if (purchaseDetailIds.Any(id => id == Guid.Empty)
                || purchaseDetailIds.Distinct().Count() != purchaseDetailIds.Count)
            {
                throw new ArgumentException("Each purchase detail must be valid and appear only once.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var purchaseDetails = await _dbContext.PurchaseDetails
                    .Where(detail => purchaseDetailIds.Contains(detail.Purchase_Detail_Id)
                        && detail.DeletedAt == null)
                    .ToListAsync();
                if (purchaseDetails.Count != purchaseDetailIds.Count)
                {
                    throw new ArgumentException("One or more purchase details were not found.");
                }

                var purchaseIds = purchaseDetails.Select(detail => detail.Purchase_Id).Distinct().ToList();
                var purchases = await _dbContext.PurchaseMaster
                    .Where(purchase => purchaseIds.Contains(purchase.Purchase_Id)
                        && purchase.DeletedAt == null)
                    .ToDictionaryAsync(purchase => purchase.Purchase_Id);
                if (purchases.Count != purchaseIds.Count)
                {
                    throw new ArgumentException("One or more linked purchases were not found.");
                }

                if (purchases.Values.Any(purchase =>
                        purchase.Company_Id != source.Company_Id
                        || purchase.Supplier_Id != source.Supplier_Id))
                {
                    throw new ArgumentException(
                        "Receipt company and supplier must match the linked purchase orders.");
                }

                var itemIds = purchaseDetails.Select(detail => detail.Item_Id).Distinct().ToList();
                var itemsById = await _dbContext.Item
                    .Where(item => itemIds.Contains(item.Item_Id))
                    .ToDictionaryAsync(item => item.Item_Id);
                if (itemsById.Count != itemIds.Count)
                {
                    throw new ArgumentException("One or more purchase items were not found.");
                }

                var now = DateTime.UtcNow;
                var receiptId = Guid.NewGuid();
                var receipt = new PurchaseReceiptMasterDTO
                {
                    PurchaseReceipt_Id = receiptId,
                    Receipt_No = source.Receipt_No,
                    Receipt_Date = source.Receipt_Date == default ? now : source.Receipt_Date,
                    Company_Id = source.Company_Id,
                    Supplier_Id = source.Supplier_Id,
                    Dc_No = source.Dc_No,
                    Dc_Date = source.Dc_Date,
                    Supplier_Invoice_No = source.Supplier_Invoice_No,
                    Supplier_Invoice_Date = source.Supplier_Invoice_Date,
                    Remarks = source.Remarks,
                    IsApproved = source.IsApproved,
                    IsCancelled = source.IsCancelled,
                    CreatedBy = source.CreatedBy,
                    CreatedAt = now
                };

                _dbContext.PurchaseReceiptMaster.Add(receipt);
                await _dbContext.SaveChangesAsync();

                var receiptDetails = new List<PurchaseReceiptDetailDTO>(sourceDetails.Count);
                var stockDetails = new List<StockDetailDTO>(sourceDetails.Count);
                foreach (var sourceDetail in sourceDetails)
                {
                    var purchaseDetail = purchaseDetails.Single(detail =>
                        detail.Purchase_Detail_Id == sourceDetail.Purchase_Detail_Id);
                    var item = itemsById[purchaseDetail.Item_Id];
                    var receivedQty = sourceDetail.Received_Qty;
                    purchaseDetail.Received_Qty += receivedQty;
                    purchaseDetail.UpdatedBy = source.CreatedBy;
                    purchaseDetail.UpdatedAt = now;

                    receiptDetails.Add(new PurchaseReceiptDetailDTO
                    {
                        PurchaseReceipt_Detail_Id = Guid.NewGuid(),
                        PurchaseReceipt_Id = receiptId,
                        Purchase_Detail_Id = purchaseDetail.Purchase_Detail_Id,
                        Received_Qty = receivedQty,
                        Accepted_Qty = sourceDetail.Accepted_Qty,
                        Rejected_Qty = sourceDetail.Rejected_Qty,
                        Shortage_Qty = sourceDetail.Shortage_Qty,
                        Rate = sourceDetail.Rate,
                        Remarks = sourceDetail.Remarks,
                        CreatedBy = source.CreatedBy,
                        CreatedAt = now
                    });

                    if (receivedQty > 0)
                    {
                        stockDetails.Add(new StockDetailDTO
                        {
                            Stock_Id = Guid.NewGuid(),
                            StockNo = receipt.Receipt_No,
                            ItemId = purchaseDetail.Item_Id,
                            ColorId = purchaseDetail.Color_Id,
                            SizeId = purchaseDetail.Size_Id,
                            Qty = receivedQty,
                            Rate = sourceDetail.Rate,
                            BalQty = receivedQty,
                            StockDate = receipt.Receipt_Date,
                            Company_Id = receipt.Company_Id,
                            Uom_Id = purchaseDetail.UOM_Id ?? item.Uom_Id,
                            SupplierId = receipt.Supplier_Id,
                            Remarks = sourceDetail.Remarks ?? receipt.Remarks,
                            MarkupRate = 0,
                            ItemCode = item.Item_Code,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = source.CreatedBy,
                            CreatedAt = now
                        });
                    }
                }

                await _dbContext.PurchaseReceiptDetails.AddRangeAsync(receiptDetails);
                await _dbContext.StockDetails.AddRangeAsync(stockDetails);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetReceipt(receiptId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(Guid receiptId, string? deletedBy)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var receipt = await _dbContext.PurchaseReceiptMaster
                    .FirstOrDefaultAsync(row => row.PurchaseReceipt_Id == receiptId
                        && row.DeletedAt == null);
                if (receipt == null)
                {
                    return false;
                }

                var details = await _dbContext.PurchaseReceiptDetails
                    .Where(row => row.PurchaseReceipt_Id == receiptId && row.DeletedAt == null)
                    .ToListAsync();
                var purchaseDetailIds = details.Select(row => row.Purchase_Detail_Id).ToList();
                var purchaseDetails = await _dbContext.PurchaseDetails
                    .Where(row => purchaseDetailIds.Contains(row.Purchase_Detail_Id))
                    .ToDictionaryAsync(row => row.Purchase_Detail_Id);
                var stockRows = await _dbContext.StockDetails
                    .Where(row => row.StockNo == receipt.Receipt_No && !row.IsDeleted)
                    .ToListAsync();

                if (stockRows.Any(row => (row.BalQty ?? row.Qty) != row.Qty))
                {
                    throw new ArgumentException(
                        "Stock from this receipt has already been used; the receipt cannot be deleted.");
                }

                var now = DateTime.UtcNow;
                foreach (var detail in details)
                {
                    if (purchaseDetails.TryGetValue(detail.Purchase_Detail_Id, out var purchaseDetail))
                    {
                        purchaseDetail.Received_Qty = Math.Max(0, purchaseDetail.Received_Qty - detail.Received_Qty);
                        purchaseDetail.UpdatedBy = deletedBy;
                        purchaseDetail.UpdatedAt = now;
                    }

                    detail.DeletedBy = deletedBy;
                    detail.DeletedAt = now;
                }

                foreach (var stock in stockRows)
                {
                    stock.IsDeleted = true;
                    stock.DeletedBy = deletedBy;
                    stock.DeletedAt = now;
                }

                receipt.DeletedBy = deletedBy;
                receipt.DeletedAt = now;

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

        public async Task<PurchaseReceiptViewModel> Update(PurchaseReceiptViewModel model)
        {
            var source = model.PurchaseReceipt_Master
                ?? throw new ArgumentException("PurchaseReceipt_Master is required.");
            var sourceDetails = model.PurchaseReceipt_Details
                ?? throw new ArgumentException("PurchaseReceipt_Details is required.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var receipt = await _dbContext.PurchaseReceiptMaster
                    .FirstOrDefaultAsync(row => row.PurchaseReceipt_Id == source.PurchaseReceipt_Id
                        && row.DeletedAt == null)
                    ?? throw new ArgumentException("Purchase receipt not found.");

                var existingDetails = await _dbContext.PurchaseReceiptDetails
                    .Where(row => row.PurchaseReceipt_Id == receipt.PurchaseReceipt_Id
                        && row.DeletedAt == null)
                    .ToListAsync();
                var purchaseDetailIds = existingDetails.Select(row => row.Purchase_Detail_Id).ToList();
                var purchaseDetails = await _dbContext.PurchaseDetails
                    .Where(row => purchaseDetailIds.Contains(row.Purchase_Detail_Id))
                    .ToDictionaryAsync(row => row.Purchase_Detail_Id);
                var stockRows = await _dbContext.StockDetails
                    .Where(row => row.StockNo == receipt.Receipt_No && !row.IsDeleted)
                    .ToListAsync();
                var itemIds = purchaseDetails.Values.Select(row => row.Item_Id).Distinct().ToList();
                var itemsById = await _dbContext.Item
                    .Where(item => itemIds.Contains(item.Item_Id))
                    .ToDictionaryAsync(item => item.Item_Id);

                var now = DateTime.UtcNow;
                receipt.Receipt_Date = source.Receipt_Date == default ? receipt.Receipt_Date : source.Receipt_Date;
                receipt.Dc_No = source.Dc_No;
                receipt.Dc_Date = source.Dc_Date;
                receipt.Supplier_Invoice_No = source.Supplier_Invoice_No;
                receipt.Supplier_Invoice_Date = source.Supplier_Invoice_Date;
                receipt.Remarks = source.Remarks;
                receipt.UpdatedBy = source.UpdatedBy ?? source.CreatedBy;
                receipt.UpdatedAt = now;

                foreach (var sourceDetail in sourceDetails)
                {
                    var existing = existingDetails.FirstOrDefault(row =>
                        row.Purchase_Detail_Id == sourceDetail.Purchase_Detail_Id)
                        ?? throw new ArgumentException("Receipt items cannot be added or removed when updating.");
                    var purchaseDetail = purchaseDetails[existing.Purchase_Detail_Id];
                    var item = itemsById[purchaseDetail.Item_Id];

                    var oldQty = existing.Received_Qty;
                    var newQty = sourceDetail.Received_Qty;
                    var delta = newQty - oldQty;

                    if (delta != 0)
                    {
                        if (purchaseDetail.Received_Qty + delta > purchaseDetail.Quantity)
                        {
                            throw new ArgumentException(
                                "Received quantity cannot exceed the ordered quantity.");
                        }

                        purchaseDetail.Received_Qty += delta;
                        purchaseDetail.UpdatedBy = receipt.UpdatedBy;
                        purchaseDetail.UpdatedAt = now;
                    }

                    existing.Received_Qty = newQty;
                    existing.Accepted_Qty = sourceDetail.Accepted_Qty;
                    existing.Rejected_Qty = sourceDetail.Rejected_Qty;
                    existing.Shortage_Qty = sourceDetail.Shortage_Qty;
                    existing.Rate = sourceDetail.Rate;
                    existing.Remarks = sourceDetail.Remarks;
                    existing.UpdatedBy = receipt.UpdatedBy;
                    existing.UpdatedAt = now;

                    var stock = stockRows.FirstOrDefault(row =>
                        row.ItemId == purchaseDetail.Item_Id
                        && row.ColorId == purchaseDetail.Color_Id
                        && row.SizeId == purchaseDetail.Size_Id);

                    if (stock == null)
                    {
                        if (newQty > 0)
                        {
                            _dbContext.StockDetails.Add(new StockDetailDTO
                            {
                                Stock_Id = Guid.NewGuid(),
                                StockNo = receipt.Receipt_No,
                                ItemId = purchaseDetail.Item_Id,
                                ColorId = purchaseDetail.Color_Id,
                                SizeId = purchaseDetail.Size_Id,
                                Qty = newQty,
                                Rate = sourceDetail.Rate,
                                BalQty = newQty,
                                StockDate = receipt.Receipt_Date,
                                Company_Id = receipt.Company_Id,
                                Uom_Id = purchaseDetail.UOM_Id ?? item.Uom_Id,
                                SupplierId = receipt.Supplier_Id,
                                Remarks = sourceDetail.Remarks ?? receipt.Remarks,
                                MarkupRate = 0,
                                ItemCode = item.Item_Code,
                                IsActive = true,
                                IsDeleted = false,
                                CreatedBy = receipt.UpdatedBy,
                                CreatedAt = now
                            });
                        }
                        continue;
                    }

                    if (delta != 0 && (stock.BalQty ?? stock.Qty) != stock.Qty)
                    {
                        throw new ArgumentException(
                            "Stock from this receipt has already been used; received quantity cannot be changed.");
                    }

                    stock.Qty = newQty;
                    stock.BalQty = newQty;
                    stock.Rate = sourceDetail.Rate;
                    stock.StockDate = receipt.Receipt_Date;
                    stock.Remarks = sourceDetail.Remarks ?? receipt.Remarks;
                    stock.IsDeleted = newQty <= 0;
                    stock.UpdatedBy = receipt.UpdatedBy;
                    stock.UpdatedAt = now;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetReceipt(receipt.PurchaseReceipt_Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
