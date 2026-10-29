using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IJobReceiptService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<JobReceiptDTO>> GetJobReceipts();
        Task<JobReceiptModel> GetJobReceipt(Guid Job_Receipt_Id);
        Task<JobReceiptModel> SaveJobReceipt(JobReceiptModel JobReceipt);
        Task<JobReceiptModel> UpdateJobReceipt(JobReceiptModel JobReceipt);
        Task<bool> DeleteJobReceipt(Guid Job_Receipt_Id);
        Task<JobReceiptItemDTO> SaveJobReceiptItem(JobReceiptItemDTO JobReceiptItem);
        Task<JobReceiptItemDTO> UpdateJobReceiptItem(JobReceiptItemDTO JobReceiptItem);
        Task<bool> DeleteJobReceiptItem(Guid Job_Receipt_Item_Id);
        Task<JobReceiptAdvancePaymentDTO> GetOpeningVoucerBalance(Guid supplierId);

    }
}
