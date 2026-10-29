using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class DespatchService : IDespatchService
    {
        private readonly IDespatchRepository _despatchRepository;

        public DespatchService(IDespatchRepository despatchRepository)
        {
            _despatchRepository = despatchRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _despatchRepository.GenerateNumber();

        public async Task<List<DespatchListDTO>> GetDespatchList(string? searchText, DateTime? fromDate, DateTime? toDate)
            => await _despatchRepository.GetDespatchList(searchText, fromDate, toDate);

        public async Task<DespatchDTO> GetDespatch(Guid productionDespatchId)
            => await _despatchRepository.GetDespatch(productionDespatchId);

        public async Task<DespatchModel> Save(DespatchModel model)
            => await _despatchRepository.Save(model);

        public async Task<DespatchModel> Update(DespatchModel model)
            => await _despatchRepository.Update(model);

        public async Task<bool> Delete(Guid productionDespatchId)
            => await _despatchRepository.Delete(productionDespatchId);
    }
}