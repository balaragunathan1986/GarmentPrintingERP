using Erp.Domain.DTO.ProductionAndLogistics.Purchase;

namespace Erp.Repository.Interfaces.Purchase
{
    public interface IOpeningStockRepository
    {
        Task<string> GenerateNumber();
        Task<List<OpeningStockViewDTO>> GetList();
        Task<OpeningStockViewDTO?> Get(string openingStockNumber);
        Task<OpeningStockViewDTO> Save(OpeningStockSaveRequestDTO request);
        Task<OpeningStockViewDTO> Update(OpeningStockSaveRequestDTO request);
        Task<bool> Delete(string openingStockNumber, string? deletedBy);
    }
}
