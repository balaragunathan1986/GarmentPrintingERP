using Erp.Domain.DTO;

namespace Erp.Repository.Interfaces.Master
{
    public interface IItemRepository
    {
        Task<List<ItemDTO>> GetItems();
        Task<ItemDTO> GetItem(Guid Item_Id);
        Task<ItemDTO> Save(ItemDTO item);
        Task<ItemDTO> Update(ItemDTO item);
        Task<bool> Delete(Guid Item_Id, string? DeletedBy);
    }
}