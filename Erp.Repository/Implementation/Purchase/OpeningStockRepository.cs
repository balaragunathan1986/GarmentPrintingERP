using Erp.Domain.DTO;
using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Purchase
{
    public class OpeningStockRepository : IOpeningStockRepository
    {
        private readonly AppDBContext _dbContext;

        public OpeningStockRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNumber()
        {
            var lastNumber = await _dbContext.OpeningStocks
                .OrderByDescending(stock => stock.OpeningStock_Number)
                .Select(stock => stock.OpeningStock_Number)
                .FirstOrDefaultAsync();

            return Utility.CommonFunctions.GenerateDocumentNumber(
                Utility.Costants.OpeningStock,
                Utility.CommonFunctions.GetTransactionNumber(lastNumber));
        }

        public Task<List<OpeningStockViewDTO>> GetList() => Read(null);

        public async Task<OpeningStockViewDTO?> Get(string openingStockNumber)
            => (await Read(openingStockNumber)).FirstOrDefault();

        public async Task<OpeningStockViewDTO> Save(OpeningStockSaveRequestDTO request)
        {
            if (await _dbContext.OpeningStocks.AnyAsync(stock =>
                    stock.OpeningStock_Number == request.OpeningStock_Number && stock.DeletedAt == null))
            {
                throw new InvalidOperationException("An opening stock entry with this number already exists.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var masters = await ResolveMasters(request);
                var now = DateTime.UtcNow;
                var openingStocks = new List<OpeningStockDTO>();
                var stockDetails = new List<StockDetailDTO>();

                foreach (var detail in request.OpeningStock_Details)
                {
                    var stockId = Guid.NewGuid();
                    stockDetails.Add(CreateStockDetail(detail.StockDetail, request, masters[detail], stockId, now));
                    openingStocks.Add(CreateOpeningStock(request, stockId, now));
                }

                await _dbContext.StockDetails.AddRangeAsync(stockDetails);
                await _dbContext.SaveChangesAsync();
                await _dbContext.OpeningStocks.AddRangeAsync(openingStocks);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await Get(request.OpeningStock_Number))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<OpeningStockViewDTO> Update(OpeningStockSaveRequestDTO request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var existing = await _dbContext.OpeningStocks
                    .Where(stock => stock.OpeningStock_Number == request.OpeningStock_Number && stock.DeletedAt == null)
                    .ToListAsync();
                if (existing.Count == 0)
                {
                    throw new KeyNotFoundException("Opening stock record not found.");
                }

                var masters = await ResolveMasters(request);
                var existingById = existing.ToDictionary(stock => stock.OpeningStock_Id);
                var existingByRowId = existing.ToDictionary(stock => stock.Id);
                var stockIds = existing.Where(stock => stock.Stock_Id.HasValue)
                    .Select(stock => stock.Stock_Id!.Value)
                    .ToList();
                var stockById = await _dbContext.StockDetails
                    .Where(stock => stockIds.Contains(stock.Stock_Id))
                    .ToDictionaryAsync(stock => stock.Stock_Id);
                var updatedRows = new HashSet<int>();
                var newOpeningStocks = new List<OpeningStockDTO>();
                var now = DateTime.UtcNow;
                var auditUser = request.UpdatedBy ?? request.CreatedBy;

                foreach (var detail in request.OpeningStock_Details)
                {
                    OpeningStockDTO? openingStock = null;
                    if (detail.OpeningStock_Id != Guid.Empty)
                    {
                        existingById.TryGetValue(detail.OpeningStock_Id, out openingStock);
                    }
                    else if (detail.Id > 0)
                    {
                        existingByRowId.TryGetValue(detail.Id, out openingStock);
                    }

                    if (detail.OpeningStock_Id != Guid.Empty || detail.Id > 0)
                    {
                        if (openingStock == null || !updatedRows.Add(openingStock.Id))
                        {
                            throw new KeyNotFoundException("Opening stock detail was not found or was submitted more than once.");
                        }

                        if (!openingStock.Stock_Id.HasValue
                            || !stockById.TryGetValue(openingStock.Stock_Id.Value, out var stock))
                        {
                            throw new InvalidOperationException("The stock record linked to an opening stock detail was not found.");
                        }

                        UpdateStockDetail(stock, detail.StockDetail, request, masters[detail], auditUser, now);
                        openingStock.OpeningStock_Date = request.OpeningStock_Date;
                        openingStock.Company_Id = request.Company_Id;
                        openingStock.Remarks = request.Remarks;
                        openingStock.UpdatedBy = auditUser;
                        openingStock.UpdatedAt = now;
                    }
                    else
                    {
                        var stockId = Guid.NewGuid();
                        _dbContext.StockDetails.Add(CreateStockDetail(detail.StockDetail, request, masters[detail], stockId, now));
                        newOpeningStocks.Add(CreateOpeningStock(request, stockId, now));
                    }
                }

                foreach (var openingStock in existing.Where(stock => !updatedRows.Contains(stock.Id)))
                {
                    openingStock.DeletedBy = auditUser;
                    openingStock.DeletedAt = now;
                    if (openingStock.Stock_Id.HasValue
                        && stockById.TryGetValue(openingStock.Stock_Id.Value, out var stock))
                    {
                        stock.IsDeleted = true;
                        stock.DeletedBy = auditUser;
                        stock.DeletedAt = now;
                    }
                }

                await _dbContext.SaveChangesAsync();
                await _dbContext.OpeningStocks.AddRangeAsync(newOpeningStocks);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return (await Get(request.OpeningStock_Number))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> Delete(string openingStockNumber, string? deletedBy)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var openingStocks = await _dbContext.OpeningStocks
                    .Where(stock => stock.OpeningStock_Number == openingStockNumber && stock.DeletedAt == null)
                    .ToListAsync();
                if (openingStocks.Count == 0)
                {
                    await transaction.CommitAsync();
                    return false;
                }

                var stockIds = openingStocks.Where(stock => stock.Stock_Id.HasValue)
                    .Select(stock => stock.Stock_Id!.Value)
                    .ToList();
                var stockDetails = await _dbContext.StockDetails
                    .Where(stock => stockIds.Contains(stock.Stock_Id))
                    .ToListAsync();
                var now = DateTime.UtcNow;

                foreach (var openingStock in openingStocks)
                {
                    openingStock.DeletedBy = deletedBy;
                    openingStock.DeletedAt = now;
                }

                foreach (var stock in stockDetails)
                {
                    stock.IsDeleted = true;
                    stock.DeletedBy = deletedBy;
                    stock.DeletedAt = now;
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

        private async Task<List<OpeningStockViewDTO>> Read(string? openingStockNumber)
        {
            var rows = await (
                from openingStock in _dbContext.OpeningStocks.AsNoTracking()
                join company in _dbContext.Company.AsNoTracking()
                    on openingStock.Company_Id equals company.Id
                join stock in _dbContext.StockDetails.AsNoTracking()
                    on openingStock.Stock_Id equals (Guid?)stock.Stock_Id
                join itemMaster in _dbContext.Item.AsNoTracking()
                    on stock.ItemId equals (Guid?)itemMaster.Item_Id into itemMasters
                from item in itemMasters.DefaultIfEmpty()
                join colorMaster in _dbContext.Color.AsNoTracking()
                    on stock.ColorId equals (Guid?)colorMaster.Color_Id into colorMasters
                from color in colorMasters.DefaultIfEmpty()
                join sizeMaster in _dbContext.Size.AsNoTracking()
                    on stock.SizeId equals (Guid?)sizeMaster.Size_Id into sizeMasters
                from size in sizeMasters.DefaultIfEmpty()
                join uomMaster in _dbContext.Uom.AsNoTracking()
                    on stock.Uom_Id equals (Guid?)uomMaster.Uom_Id into uomMasters
                from uom in uomMasters.DefaultIfEmpty()
                where openingStock.DeletedAt == null
                    && !stock.IsDeleted
                    && (openingStockNumber == null || openingStock.OpeningStock_Number == openingStockNumber)
                orderby openingStock.Id descending
                select new OpeningStockViewDTO
                {
                    Id = openingStock.Id,
                    OpeningStock_Number = openingStock.OpeningStock_Number,
                    OpeningStock_Date = openingStock.OpeningStock_Date,
                    Company_Id = openingStock.Company_Id,
                    CompanyName = company.Company_Name ?? string.Empty,
                    Remarks = openingStock.Remarks,
                    CreatedBy = openingStock.CreatedBy,
                    CreatedAt = openingStock.CreatedAt,
                    UpdatedBy = openingStock.UpdatedBy,
                    UpdatedAt = openingStock.UpdatedAt,
                    Details = new List<OpeningStockLineViewDTO>
                    {
                        new()
                        {
                            Id = openingStock.Id,
                            OpeningStock_Id = openingStock.OpeningStock_Id,
                            Stock_Id = stock.Stock_Id,
                            StockNo = stock.StockNo,
                            Item_Id = item == null ? Guid.Empty : item.Item_Id,
                            Item_Code = item == null ? stock.ItemCode : item.Item_Code,
                            Item_Name = item == null ? string.Empty : item.Item_Name,
                            Item_Type = item == null ? string.Empty : item.Item_Type,
                            Color_Id = stock.ColorId,
                            Color_Code = color == null ? string.Empty : color.Color_Code ?? string.Empty,
                            Color_Name = color == null ? string.Empty : color.Color_Name ?? string.Empty,
                            Size_Id = stock.SizeId,
                            Size_Code = size == null ? string.Empty : size.Size_Code ?? string.Empty,
                            Size_Name = size == null ? string.Empty : size.Size_Name ?? string.Empty,
                            Qty = stock.Qty,
                            Rate = stock.Rate,
                            Uom_Id = stock.Uom_Id,
                            Uom_Name = uom == null ? string.Empty : uom.Uom ?? string.Empty,
                            Uom_Abbreviation = uom == null ? string.Empty : uom.Abbrevation ?? string.Empty,
                            Remarks = stock.Remarks ?? string.Empty
                        }
                    }
                }).ToListAsync();

            return rows.GroupBy(row => row.OpeningStock_Number)
                .Select(group =>
                {
                    var document = group.First();
                    document.Details = group.SelectMany(row => row.Details).ToList();
                    return document;
                })
                .ToList();
        }

        private async Task<Dictionary<OpeningStockDetailSaveRequestDTO, ResolvedStockMasters>> ResolveMasters(
            OpeningStockSaveRequestDTO request)
        {
            ValidateRequest(request);

            var company = await _dbContext.Company.AsNoTracking()
                .FirstOrDefaultAsync(master => master.Id == request.Company_Id);
            if (company == null || company.Company_Id != request.Company_GUID)
            {
                throw new ArgumentException("Company_Id and Company_GUID must identify the same existing company.");
            }

            var details = request.OpeningStock_Details;
            var uomIds = details.Select(detail => GetUomId(detail)).OfType<Guid>().Distinct().ToList();
            var uomNames = details.Select(detail => Clean(detail.Uom_Name)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var uomAbbreviations = details.Select(detail => Clean(detail.Uom_Abbreviation)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var uoms = await _dbContext.Uom.Where(uom =>
                    uomIds.Contains(uom.Uom_Id)
                    || (uom.Uom != null && uomNames.Contains(uom.Uom.ToLower()))
                    || (uom.Abbrevation != null && uomAbbreviations.Contains(uom.Abbrevation.ToLower())))
                .Where(uom => !uom.IsDeleted)
                .ToListAsync();
            var uomsById = uoms.ToDictionary(uom => uom.Uom_Id);
            var uomsByName = IndexBy(uoms, uom => uom.Uom);
            var uomsByAbbreviation = IndexBy(uoms, uom => uom.Abbrevation);
            var uomByDetail = new Dictionary<OpeningStockDetailSaveRequestDTO, UomDTO?>();

            foreach (var detail in details)
            {
                var id = GetUomId(detail);
                var name = Clean(detail.Uom_Name);
                var abbreviation = Clean(detail.Uom_Abbreviation);
                UomDTO? uom = null;
                if (id.HasValue)
                {
                    uomsById.TryGetValue(id.Value, out uom);
                }

                if (uom == null && abbreviation != null)
                {
                    uomsByAbbreviation.TryGetValue(abbreviation, out uom);
                }

                if (uom == null && name != null)
                {
                    uomsByName.TryGetValue(name, out uom);
                }

                if (uom == null && (id.HasValue || name != null || abbreviation != null))
                {
                    if (name == null && abbreviation == null)
                    {
                        throw new ArgumentException("A name or abbreviation is required to create a missing unit of measurement.");
                    }

                    uom = new UomDTO
                    {
                        Uom_Id = Guid.NewGuid(),
                        Uom = name ?? abbreviation!,
                        Abbrevation = abbreviation ?? name,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Uom.Add(uom);
                    uomsById.Add(uom.Uom_Id, uom);
                    AddIndex(uomsByName, uom.Uom, uom);
                    AddIndex(uomsByAbbreviation, uom.Abbrevation, uom);
                }

                uomByDetail.Add(detail, uom);
            }

            await _dbContext.SaveChangesAsync();

            var itemIds = details.Select(GetItemId).OfType<Guid>().Distinct().ToList();
            var itemCodes = details.Select(GetItemCode).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var itemNames = details.Select(detail => Clean(detail.Item_Name)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var items = await _dbContext.Item.Where(item =>
                    itemIds.Contains(item.Item_Id)
                    || itemCodes.Contains(item.Item_Code.ToLower())
                    || itemNames.Contains(item.Item_Name.ToLower()))
                .Where(item => item.DeletedAt == null)
                .ToListAsync();
            var itemsById = items.ToDictionary(item => item.Item_Id);
            var itemsByCode = IndexBy(items, item => item.Item_Code);
            var itemsByName = IndexBy(items, item => item.Item_Name);
            var itemByDetail = new Dictionary<OpeningStockDetailSaveRequestDTO, ItemDTO>();

            foreach (var detail in details)
            {
                var id = GetItemId(detail);
                var code = GetItemCode(detail);
                var name = Clean(detail.Item_Name);
                ItemDTO? item = null;
                if (id.HasValue)
                {
                    itemsById.TryGetValue(id.Value, out item);
                }

                if (item == null && code != null)
                {
                    itemsByCode.TryGetValue(code, out item);
                }

                if (item == null && name != null)
                {
                    itemsByName.TryGetValue(name, out item);
                }

                if (item == null)
                {
                    if (code == null || name == null)
                    {
                        throw new ArgumentException("A new item requires both an item code and an item name.");
                    }

                    item = new ItemDTO
                    {
                        Item_Id = Guid.NewGuid(),
                        Item_Code = code,
                        Item_Name = name,
                        Item_Type = Clean(detail.Item_Type) ?? "Material",
                        Uom_Id = uomByDetail[detail]?.Uom_Id,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Item.Add(item);
                    itemsById.Add(item.Item_Id, item);
                    AddIndex(itemsByCode, item.Item_Code, item);
                    AddIndex(itemsByName, item.Item_Name, item);
                }

                itemByDetail.Add(detail, item);
            }

            await _dbContext.SaveChangesAsync();

            var colorIds = details.Select(GetColorId).OfType<Guid>().Distinct().ToList();
            var colorCodes = details.Select(detail => Clean(detail.Color_Code)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var colorNames = details.Select(detail => Clean(detail.Color_Name)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var colors = await _dbContext.Color.Where(color =>
                    colorIds.Contains(color.Color_Id)
                    || (color.Color_Code != null && colorCodes.Contains(color.Color_Code.ToLower()))
                    || (color.Color_Name != null && colorNames.Contains(color.Color_Name.ToLower())))
                .Where(color => !color.IsDeleted)
                .ToListAsync();
            var colorsById = colors.ToDictionary(color => color.Color_Id);
            var colorsByCode = IndexBy(colors, color => color.Color_Code);
            var colorsByName = IndexBy(colors, color => color.Color_Name);
            var sizeIds = details.Select(GetSizeId).OfType<Guid>().Distinct().ToList();
            var sizeCodes = details.Select(detail => Clean(detail.Size_Code)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var sizeNames = details.Select(detail => Clean(detail.Size_Name)).OfType<string>()
                .Select(value => value.ToLowerInvariant()).Distinct().ToList();
            var sizes = await _dbContext.Size.Where(size =>
                    sizeIds.Contains(size.Size_Id)
                    || (size.Size_Code != null && sizeCodes.Contains(size.Size_Code.ToLower()))
                    || (size.Size_Name != null && sizeNames.Contains(size.Size_Name.ToLower())))
                .Where(size => !size.IsDeleted)
                .ToListAsync();
            var sizesById = sizes.ToDictionary(size => size.Size_Id);
            var sizesByCode = IndexBy(sizes, size => size.Size_Code);
            var sizesByName = IndexBy(sizes, size => size.Size_Name);
            var colorByDetail = new Dictionary<OpeningStockDetailSaveRequestDTO, ColorDTO?>();
            var sizeByDetail = new Dictionary<OpeningStockDetailSaveRequestDTO, SizeDTO?>();

            foreach (var detail in details)
            {
                var colorId = GetColorId(detail);
                var colorCode = Clean(detail.Color_Code);
                var colorName = Clean(detail.Color_Name);
                ColorDTO? color = null;
                if (colorId.HasValue)
                {
                    colorsById.TryGetValue(colorId.Value, out color);
                }

                if (color == null && colorCode != null)
                {
                    colorsByCode.TryGetValue(colorCode, out color);
                }

                if (color == null && colorName != null)
                {
                    colorsByName.TryGetValue(colorName, out color);
                }

                if (color == null && colorId.HasValue && colorCode == null && colorName == null)
                {
                    throw new ArgumentException("A code or name is required to create a missing color.");
                }

                if (color == null && (colorCode != null || colorName != null))
                {
                    color = new ColorDTO
                    {
                        Color_Id = Guid.NewGuid(),
                        Color_Code = colorCode,
                        Color_Name = colorName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Color.Add(color);
                    colorsById.Add(color.Color_Id, color);
                    AddIndex(colorsByCode, color.Color_Code, color);
                    AddIndex(colorsByName, color.Color_Name, color);
                }

                var sizeId = GetSizeId(detail);
                var sizeCode = Clean(detail.Size_Code);
                var sizeName = Clean(detail.Size_Name);
                SizeDTO? size = null;
                if (sizeId.HasValue)
                {
                    sizesById.TryGetValue(sizeId.Value, out size);
                }

                if (size == null && sizeCode != null)
                {
                    sizesByCode.TryGetValue(sizeCode, out size);
                }

                if (size == null && sizeName != null)
                {
                    sizesByName.TryGetValue(sizeName, out size);
                }

                if (size == null && sizeId.HasValue && sizeCode == null && sizeName == null)
                {
                    throw new ArgumentException("A code or name is required to create a missing size.");
                }

                if (size == null && (sizeCode != null || sizeName != null))
                {
                    size = new SizeDTO
                    {
                        Size_Id = Guid.NewGuid(),
                        Size_Code = sizeCode,
                        Size_Name = sizeName,
                        Size_Group_Id = NormalizeId(detail.Size_Group_Id) ?? Guid.NewGuid(),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = request.CreatedBy,
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Size.Add(size);
                    sizesById.Add(size.Size_Id, size);
                    AddIndex(sizesByCode, size.Size_Code, size);
                    AddIndex(sizesByName, size.Size_Name, size);
                }

                colorByDetail.Add(detail, color);
                sizeByDetail.Add(detail, size);
            }

            await _dbContext.SaveChangesAsync();

            return details.ToDictionary(
                detail => detail,
                detail => new ResolvedStockMasters(
                    itemByDetail[detail].Item_Id,
                    itemByDetail[detail].Item_Code,
                    colorByDetail[detail]?.Color_Id,
                    sizeByDetail[detail]?.Size_Id,
                    uomByDetail[detail]?.Uom_Id ?? itemByDetail[detail].Uom_Id));
        }

        private static void ValidateRequest(OpeningStockSaveRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.OpeningStock_Number))
            {
                throw new ArgumentException("OpeningStock_Number is required.");
            }

            if (string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                throw new ArgumentException("CreatedBy is required.");
            }

            if (request.OpeningStock_Date == default)
            {
                throw new ArgumentException("OpeningStock_Date is required.");
            }

            if (request.OpeningStock_Details == null || request.OpeningStock_Details.Count == 0)
            {
                throw new ArgumentException("At least one opening stock detail is required.");
            }

            if (request.OpeningStock_Details.Any(detail => detail == null || detail.StockDetail == null))
            {
                throw new ArgumentException("Every opening stock detail must include stock quantity and rate information.");
            }

            foreach (var detail in request.OpeningStock_Details)
            {
                if (detail.StockDetail.Qty < 0)
                {
                    throw new ArgumentException("Opening stock quantities cannot be negative.");
                }

                if (detail.StockDetail.Rate.HasValue && detail.StockDetail.Rate.Value < 0)
                {
                    throw new ArgumentException("Opening stock rates cannot be negative.");
                }
            }
        }

        private static Guid? GetItemId(OpeningStockDetailSaveRequestDTO detail)
            => NormalizeId(detail.Item_Id) ?? NormalizeId(detail.StockDetail.ItemId);

        private static Guid? GetColorId(OpeningStockDetailSaveRequestDTO detail)
            => NormalizeId(detail.Color_Id) ?? NormalizeId(detail.StockDetail.ColorId);

        private static Guid? GetSizeId(OpeningStockDetailSaveRequestDTO detail)
            => NormalizeId(detail.Size_Id) ?? NormalizeId(detail.StockDetail.SizeId);

        private static Guid? GetUomId(OpeningStockDetailSaveRequestDTO detail)
            => NormalizeId(detail.Uom_Id) ?? NormalizeId(detail.StockDetail.Uom_Id);

        private static string? GetItemCode(OpeningStockDetailSaveRequestDTO detail)
            => Clean(detail.Item_Code) ?? Clean(detail.StockDetail.ItemCode);

        private static Guid? NormalizeId(Guid? id)
            => id.HasValue && id.Value != Guid.Empty ? id : null;

        private static string? Clean(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static Dictionary<string, T> IndexBy<T>(IEnumerable<T> rows, Func<T, string?> selector)
        {
            return rows.Select(row => new { Row = row, Key = Clean(selector(row)) })
                .Where(item => item.Key != null)
                .GroupBy(item => item.Key!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Row, StringComparer.OrdinalIgnoreCase);
        }

        private static void AddIndex<T>(Dictionary<string, T> index, string? key, T value)
        {
            key = Clean(key);
            if (key != null && !index.ContainsKey(key))
            {
                index.Add(key, value);
            }
        }

        private sealed record ResolvedStockMasters(
            Guid ItemId,
            string ItemCode,
            Guid? ColorId,
            Guid? SizeId,
            Guid? UomId);

        private static OpeningStockDTO CreateOpeningStock(
            OpeningStockSaveRequestDTO request,
            Guid stockId,
            DateTime now)
            => new()
            {
                OpeningStock_Id = Guid.NewGuid(),
                OpeningStock_Number = request.OpeningStock_Number,
                OpeningStock_Date = request.OpeningStock_Date,
                Company_Id = request.Company_Id,
                Stock_Id = stockId,
                CreatedBy = request.CreatedBy,
                CreatedAt = now,
                Remarks = request.Remarks
            };

        private static StockDetailDTO CreateStockDetail(
            StockDetailDTO source,
            OpeningStockSaveRequestDTO request,
            ResolvedStockMasters masters,
            Guid stockId,
            DateTime now)
        {
            return new StockDetailDTO
            {
                Stock_Id = stockId,
                StockNo = request.OpeningStock_Number,
                ItemId = masters.ItemId,
                ColorId = masters.ColorId,
                SizeId = masters.SizeId,
                Qty = source.Qty,
                Rate = source.Rate,
                AllotedQty = 0,
                SQty = source.Qty,
                BalQty = source.Qty,
                StockDate = request.OpeningStock_Date,
                Company_Id = request.Company_GUID,
                ReturnQty = 0,
                Uom_Id = masters.UomId,
                Remarks = source.Remarks,
                MarkupRate = 0,
                ItemCode = masters.ItemCode,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = request.CreatedBy,
                CreatedAt = now
            };
        }

        private static void UpdateStockDetail(
            StockDetailDTO stock,
            StockDetailDTO source,
            OpeningStockSaveRequestDTO request,
            ResolvedStockMasters masters,
            string auditUser,
            DateTime now)
        {
            stock.StockNo = request.OpeningStock_Number;
            stock.ItemId = masters.ItemId;
            stock.ColorId = masters.ColorId;
            stock.SizeId = masters.SizeId;
            stock.Qty = source.Qty;
            stock.Rate = source.Rate;
            stock.SQty = source.Qty;
            stock.BalQty = source.Qty;
            stock.StockDate = request.OpeningStock_Date;
            stock.Company_Id = request.Company_GUID;
            stock.Uom_Id = masters.UomId;
            stock.Remarks = source.Remarks;
            stock.ItemCode = masters.ItemCode;
            stock.IsActive = true;
            stock.IsDeleted = false;
            stock.UpdatedBy = auditUser;
            stock.UpdatedAt = now;
            stock.DeletedBy = null;
            stock.DeletedAt = null;
        }
    }
}
