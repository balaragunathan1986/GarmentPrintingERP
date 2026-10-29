using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;

        public QuotationService(IQuotationRepository quotationRepository)
        {
            _quotationRepository = quotationRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _quotationRepository.GenerateNumber();

        public async Task<List<QuotationMasterDTO>> GetQuotations()
            => await _quotationRepository.GetQuotations();

        public async Task<QuotationMasterDTO> GetQuotation(Guid quotationId)
            => await _quotationRepository.GetQuotation(quotationId);

        public async Task<QuotationMasterDTO> Save(QuotationModel model)
            => await _quotationRepository.Save(model);

        public async Task<QuotationMasterDTO> Update(QuotationModel model)
            => await _quotationRepository.Update(model);

        public async Task<bool> Delete(Guid quotationId, string? deletedBy)
            => await _quotationRepository.Delete(quotationId, deletedBy);
    }
}