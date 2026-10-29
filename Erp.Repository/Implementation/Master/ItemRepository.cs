using Erp.Domain.DTO;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Master;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Master
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDBContext _dbcontext;

        public ItemRepository(AppDBContext context)
        {
            _dbcontext = context;
        }

        public async Task<List<ItemDTO>> GetItems()
        {
            return await _dbcontext.Item.Where(item => item.DeletedAt == null).ToListAsync();
        }

        public async Task<ItemDTO> GetItem(Guid Item_Id)
        {
            return await _dbcontext.Item.FirstOrDefaultAsync(item => item.Item_Id == Item_Id && item.DeletedAt == null)
                ?? new ItemDTO();
        }

        public async Task<ItemDTO> Save(ItemDTO item)
        {
            await _dbcontext.Item.AddAsync(item);
            await _dbcontext.SaveChangesAsync();
            return item;
        }

        public async Task<ItemDTO> Update(ItemDTO item)
        {
            var existingItem = await _dbcontext.Item.FirstOrDefaultAsync(existing =>
                existing.Item_Id == item.Item_Id && existing.Id == item.Id && existing.DeletedAt == null);

            if (existingItem == null)
            {
                return new ItemDTO();
            }

            existingItem.Item_Code = item.Item_Code;
            existingItem.Item_Name = item.Item_Name;
            existingItem.Item_Type = item.Item_Type;
            existingItem.Uom_Id = item.Uom_Id;
            existingItem.Hsn_Code = item.Hsn_Code;
            existingItem.Description = item.Description;
            existingItem.Barcode = item.Barcode;
            existingItem.Customer_Item_Code = item.Customer_Item_Code;
            existingItem.UpdatedBy = item.UpdatedBy;
            existingItem.UpdatedAt = item.UpdatedAt;

            await _dbcontext.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> Delete(Guid Item_Id, string? DeletedBy)
        {
            var item = await _dbcontext.Item.FirstOrDefaultAsync(existing =>
                existing.Item_Id == Item_Id && existing.DeletedAt == null);

            if (item == null)
            {
                return false;
            }

            item.DeletedBy = DeletedBy;
            item.DeletedAt = DateTime.UtcNow;
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}