using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IBillingRepository
    {
        Task<string> GenerateNumber();
        Task<List<BillingListDTO>> GetBillingList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<BillingMasterDTO> GetBilling(Guid billingId);
        Task<BillingModel> Save(BillingModel model);
        Task<BillingModel> Update(BillingModel model);
        Task<bool> Delete(Guid billingId);
        Task<BillingReportDTO> GetBillingReport(Guid billingId);
    }
}