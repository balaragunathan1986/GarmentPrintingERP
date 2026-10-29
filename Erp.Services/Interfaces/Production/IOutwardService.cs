using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IOutwardService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<OutwardDTO>> GetOutwards();
        Task<OutwardDTO> GetOutward(Guid outwardId);
        Task<List<InwardMasterDTO>> GetAvailableInwards();
        Task<List<OutwardDetailDTO>> GetInwardItems(Guid inwardId);
        Task<OutwardDTO> Save(OutwardModel model);
        Task<OutwardDTO> Update(OutwardModel model);
        Task<bool> Delete(Guid outwardId, string? deletedBy);
    }
}