using Erp.Domain.DTO.ProductionAndLogistics.Purchase;

namespace Erp.Services.Interfaces.Purchase
{
    public interface IOpeningStockService
    {
        Task<string> GenerateNumber();
        Task<List<OpeningStockViewDTO>> GetList();
        Task<OpeningStockViewDTO?> Get(string openingStockNumber);
        Task<OpeningStockViewDTO> Save(OpeningStockSaveRequestDTO request);
        Task<OpeningStockViewDTO> Update(OpeningStockSaveRequestDTO request);
        Task<bool> Delete(string openingStockNumber, string? deletedBy);
    }
}
