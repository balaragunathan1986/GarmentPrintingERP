using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IIssueService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<IssueMasterDTO>> GetIssues();
        Task<IssueMasterDTO> GetIssue(Guid issueId);
        Task<IssueMasterDTO> Save(IssueModel model);
        Task<IssueMasterDTO> Update(IssueModel model);
        Task<bool> Delete(Guid issueId, string? deletedBy);
    }
}
