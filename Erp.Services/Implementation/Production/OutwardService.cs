using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class OutwardService : IOutwardService
    {
        private readonly IOutwardRepository _outwardRepository;

        public OutwardService(IOutwardRepository outwardRepository)
        {
            _outwardRepository = outwardRepository;
        }

        public Task<string> GenerateDocumentNumber() => _outwardRepository.GenerateNumber();
        public Task<List<OutwardDTO>> GetOutwards() => _outwardRepository.GetOutwards();
        public Task<OutwardDTO> GetOutward(Guid outwardId) => _outwardRepository.GetOutward(outwardId);
        public Task<List<InwardMasterDTO>> GetAvailableInwards() => _outwardRepository.GetAvailableInwards();
        public Task<List<OutwardDetailDTO>> GetInwardItems(Guid inwardId) => _outwardRepository.GetInwardItems(inwardId);
        public Task<OutwardDTO> Save(OutwardModel model) => _outwardRepository.Save(model);
        public Task<OutwardDTO> Update(OutwardModel model) => _outwardRepository.Update(model);
        public Task<bool> Delete(Guid outwardId, string? deletedBy) => _outwardRepository.Delete(outwardId, deletedBy);
    }
}