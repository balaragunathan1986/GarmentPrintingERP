using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class IssueService : IIssueService
    {
        private readonly IIssueRepository _issueRepository;

        public IssueService(IIssueRepository issueRepository)
        {
            _issueRepository = issueRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _issueRepository.GenerateNumber();

        public async Task<List<IssueMasterDTO>> GetIssues()
            => await _issueRepository.GetIssues();

        public async Task<IssueMasterDTO> GetIssue(Guid issueId)
            => await _issueRepository.GetIssue(issueId);

        public async Task<IssueMasterDTO> Save(IssueModel model)
            => await _issueRepository.Save(model);

        public async Task<IssueMasterDTO> Update(IssueModel model)
            => await _issueRepository.Update(model);

        public async Task<bool> Delete(Guid issueId, string? deletedBy)
            => await _issueRepository.Delete(issueId, deletedBy);
    }
}
