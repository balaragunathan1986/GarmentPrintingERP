using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Implementation.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class ProductionBatchService : IProductionBatchService
    {
        private readonly IProductionBatchRepository _productionBatchRepository;

        public async Task<string> GenerateDocumentNumber() => await _productionBatchRepository.GenerateNumber();
        public ProductionBatchService(IProductionBatchRepository productionBatchRepository)
        {
            _productionBatchRepository = productionBatchRepository;
        }

        public async Task<List<JobReceiptListDTO>> GetOpenJobReceipts(DateTime? fromDate, DateTime? toDate)
            => await _productionBatchRepository.GetOpenJobReceipts(fromDate, toDate);

        public async Task<List<JobReceiptListDTO>> GetEditJobReceipts(Guid Production_Batch_Id,DateTime? fromDate, DateTime? toDate)
       => await _productionBatchRepository.GetEditJobReceipts(Production_Batch_Id,fromDate, toDate);


        public async Task<List<ProductionBatchListDTO>> GetProductionBatchList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            return await _productionBatchRepository.GetProductionBatchList(searchText, fromDate, toDate);
        }

        public async Task<List<ProductionBatchListDTO>> GetProductionBatchCostList(string? searchText, DateTime? fromDate, DateTime? toDate)
        {
            return await _productionBatchRepository.GetProductionBatchCostList(searchText, fromDate, toDate);
        }

        public async Task<List<ProductionBatchDTO>> GetProductionBatch()
        {
            return await _productionBatchRepository.GetProductionBatch();
        }

        public async Task<ProductionBatchDTO> GetProductionBatch(Guid Production_Batch_Id)
        {
            return await _productionBatchRepository.GetProductionBatch(Production_Batch_Id);
        }

        public async Task<ProductionBatchModel> Save(ProductionBatchModel model)
        {
            return await _productionBatchRepository.Save(model);
        }

        public async Task<ProductionBatchModel> Update(ProductionBatchModel model)
        {
            return await _productionBatchRepository.Update(model);
        }

        public async Task<bool> Delete(Guid Production_Batch_Id)
        {
            return await _productionBatchRepository.Delete(Production_Batch_Id);
        }
    }
}
