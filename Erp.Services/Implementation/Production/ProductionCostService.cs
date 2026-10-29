using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class ProductionCostService : IProductionCostService
    {
        private readonly IProductionCostRepository _productionCostRepository;

        public ProductionCostService(IProductionCostRepository productionCostRepository)
        {
            _productionCostRepository = productionCostRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _productionCostRepository.GenerateNumber();

        public async Task<List<ProductionCostListDTO>> GetProductionCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
            => await _productionCostRepository.GetProductionCostList(searchText, fromDate, toDate);

        public async Task<List<ProductionCostListDTO>> GetProductionSelectedCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
       => await _productionCostRepository.GetProductionSelectedCostList(searchText, fromDate, toDate);

        public async Task<List<ProductionCostDTO>> GetProductionCost()
            => await _productionCostRepository.GetProductionCost();

        public async Task<ProductionCostDTO> GetProductionCost(Guid productionCostId)
            => await _productionCostRepository.GetProductionCost(productionCostId);

        public async Task<ProductionCostModel> Save(ProductionCostModel model)
            => await _productionCostRepository.Save(model);

        public async Task<ProductionCostModel> Update(ProductionCostModel model)
            => await _productionCostRepository.Update(model);

        public async Task<bool> Delete(Guid productionCostId)
            => await _productionCostRepository.Delete(productionCostId);
        
    }
}
