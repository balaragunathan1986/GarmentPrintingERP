using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IJobOrderService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<JobOrderMasterDTO>> GetJobOrders();
        Task<JobOrderMasterDTO> GetJobOrder(Guid jobOrderId);
        Task<JobOrderMasterDTO> Save(JobOrderModel model);
        Task<JobOrderMasterDTO> Update(JobOrderModel model);
        Task<bool> Delete(Guid jobOrderId, string? deletedBy);
    }
}
