using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IJobOrderRepository
    {
        Task<string> GenerateNumber();
        Task<List<JobOrderMasterDTO>> GetJobOrders();
        Task<JobOrderMasterDTO> GetJobOrder(Guid jobOrderId);
        Task<JobOrderMasterDTO> Save(JobOrderModel model);
        Task<JobOrderMasterDTO> Update(JobOrderModel model);
        Task<bool> Delete(Guid jobOrderId, string? deletedBy);
    }
}
