using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class JobOrderService : IJobOrderService
    {
        private readonly IJobOrderRepository _jobOrderRepository;

        public JobOrderService(IJobOrderRepository jobOrderRepository)
        {
            _jobOrderRepository = jobOrderRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _jobOrderRepository.GenerateNumber();

        public async Task<List<JobOrderMasterDTO>> GetJobOrders()
            => await _jobOrderRepository.GetJobOrders();

        public async Task<JobOrderMasterDTO> GetJobOrder(Guid jobOrderId)
            => await _jobOrderRepository.GetJobOrder(jobOrderId);

        public async Task<JobOrderMasterDTO> Save(JobOrderModel model)
            => await _jobOrderRepository.Save(model);

        public async Task<JobOrderMasterDTO> Update(JobOrderModel model)
            => await _jobOrderRepository.Update(model);

        public async Task<bool> Delete(Guid jobOrderId, string? deletedBy)
            => await _jobOrderRepository.Delete(jobOrderId, deletedBy);
    }
}
