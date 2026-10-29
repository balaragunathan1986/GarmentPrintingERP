using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IProductionBatchService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<JobReceiptListDTO>> GetOpenJobReceipts(DateTime? fromDate, DateTime? toDate);
        Task<List<JobReceiptListDTO>> GetEditJobReceipts(Guid Production_Batch_Id,DateTime? fromDate, DateTime? toDate);
        Task<List<ProductionBatchListDTO>> GetProductionBatchList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<List<ProductionBatchListDTO>> GetProductionBatchCostList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<List<ProductionBatchDTO>> GetProductionBatch();
        Task<ProductionBatchDTO> GetProductionBatch(Guid Production_Batch_Id);
        Task<ProductionBatchModel> Save(ProductionBatchModel model);
        Task<ProductionBatchModel> Update(ProductionBatchModel model);
        Task<bool> Delete(Guid Production_Batch_Id);
    }
}
