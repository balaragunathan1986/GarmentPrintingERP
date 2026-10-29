using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Master;
using Erp.Services.Interfaces.Master;

namespace Erp.Services.Implementation.Master
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        public async Task<List<ItemDTO>> GetItems()
        {
            return await _itemRepository.GetItems();
        }

        public async Task<ItemDTO> GetItem(Guid Item_Id)
        {
            return await _itemRepository.GetItem(Item_Id);
        }

        public async Task<ItemDTO> Save(ItemDTO item)
        {
            item.Item_Id = Guid.NewGuid();
            return await _itemRepository.Save(item);
        }

        public async Task<ItemDTO> Update(ItemDTO item)
        {
            return await _itemRepository.Update(item);
        }

        public async Task<bool> Delete(Guid Item_Id, string? DeletedBy)
        {
            return await _itemRepository.Delete(Item_Id, DeletedBy);
        }
    }
}