using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IDespatchService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<DespatchListDTO>> GetDespatchList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<DespatchDTO> GetDespatch(Guid productionDespatchId);
        Task<DespatchModel> Save(DespatchModel model);
        Task<DespatchModel> Update(DespatchModel model);
        Task<bool> Delete(Guid productionDespatchId);
    }
}