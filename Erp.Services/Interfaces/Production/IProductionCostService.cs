using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IProductionCostService
    {
        Task<string> GenerateDocumentNumber();  
        Task<List<ProductionCostDTO>> GetProductionCost();
        Task<ProductionCostDTO> GetProductionCost(Guid productionCostId);
        Task<List<ProductionCostListDTO>> GetProductionCostList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<List<ProductionCostListDTO>> GetProductionSelectedCostList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<ProductionCostModel> Save(ProductionCostModel model);
        Task<ProductionCostModel> Update(ProductionCostModel model);
        Task<bool> Delete(Guid productionCostId);
    }
}
