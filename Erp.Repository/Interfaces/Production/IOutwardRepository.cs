using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IOutwardRepository
    {
        Task<string> GenerateNumber();
        Task<List<OutwardDTO>> GetOutwards();
        Task<OutwardDTO> GetOutward(Guid outwardId);
        Task<List<InwardMasterDTO>> GetAvailableInwards();
        Task<List<OutwardDetailDTO>> GetInwardItems(Guid inwardId);
        Task<OutwardDTO> Save(OutwardModel model);
        Task<OutwardDTO> Update(OutwardModel model);
        Task<bool> Delete(Guid outwardId, string? deletedBy);
    }
}