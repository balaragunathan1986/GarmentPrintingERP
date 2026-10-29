using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IQuotationRepository
    {
        Task<string> GenerateNumber();
        Task<List<QuotationMasterDTO>> GetQuotations();
        Task<QuotationMasterDTO> GetQuotation(Guid quotationId);
        Task<QuotationMasterDTO> Save(QuotationModel model);
        Task<QuotationMasterDTO> Update(QuotationModel model);
        Task<bool> Delete(Guid quotationId, string? deletedBy);
    }
}