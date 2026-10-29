using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IProductionCostRepository
    {
        Task<string> GenerateNumber();
        Task<List<ProductionCostListDTO>> GetProductionCostList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<List<ProductionCostListDTO>> GetProductionSelectedCostList(string? searchText, DateTime? fromDate, DateTime? toDate);

        Task<List<ProductionCostDTO>> GetProductionCost();
        Task<ProductionCostDTO> GetProductionCost(Guid productionCostId);
        Task<ProductionCostModel> Save(ProductionCostModel model);
        Task<ProductionCostModel> Update(ProductionCostModel model);
        Task<bool> Delete(Guid productionCostId);
    }
}
