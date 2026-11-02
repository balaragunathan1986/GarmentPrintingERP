using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IIssueRepository
    {
        Task<string> GenerateNumber();
        Task<List<IssueMasterDTO>> GetIssues();
        Task<IssueMasterDTO> GetIssue(Guid issueId);
        Task<IssueMasterDTO> Save(IssueModel model);
        Task<IssueMasterDTO> Update(IssueModel model);
        Task<bool> Delete(Guid issueId, string? deletedBy);
    }
}
