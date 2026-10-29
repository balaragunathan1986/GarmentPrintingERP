using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IQuotationService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<QuotationMasterDTO>> GetQuotations();
        Task<QuotationMasterDTO> GetQuotation(Guid quotationId);
        Task<QuotationMasterDTO> Save(QuotationModel model);
        Task<QuotationMasterDTO> Update(QuotationModel model);
        Task<bool> Delete(Guid quotationId, string? deletedBy);
    }
}