using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Repository.Interfaces.Production
{
    public interface IJobReceiptRepository
    {
        #region Job receipt Master
        Task<string> GenerateNumber();
        Task<List<JobReceiptDTO>> GetJobReceipt();
        Task<JobReceiptDTO> GetJobReceipt(Guid Job_Rceipt_Id);
        Task<JobReceiptDTO> Save(JobReceiptDTO JobReceipt);
        Task<JobReceiptDTO> Update(JobReceiptDTO JobReceipt);
        Task<bool> Delete(Guid Job_Receipt_Id);
        #endregion

        #region Job Receipt Item details
        Task<List<JobReceiptItemDTO>> GetJobReceiptItems();
        Task<List<JobReceiptItemDTO>> GetJobReceiptItems(Guid Job_Receipt_Id);
        Task<JobReceiptItemDTO> Save(JobReceiptItemDTO JobReceiptItem);
        Task<bool> Save(List<JobReceiptItemDTO> JobReceiptItems);
        Task<bool> Update(List<JobReceiptItemDTO> JobReceiptItems);
        Task<JobReceiptItemDTO> Update(JobReceiptItemDTO JobReceiptItem);
        Task<bool> DeleteItems(Guid Job_Receipt_Id);
        Task<bool> DeleteJobReceiptItem(Guid Job_Receipt_Item_Id);

        Task<JobReceiptAdvancePaymentDTO> GetAdvancePayment(Guid SupplierId);

        #endregion

        #region Job Receipt Charges
        Task<List<JobReceiptChargeDTO>> GetCharges(Guid Job_Receipt_Id);
        Task<List<JobReceiptChargeDTO>> SaveCharges(List<JobReceiptChargeDTO> JobReceiptCharges);
        Task<List<JobReceiptChargeDTO>> UpdateCharges(List<JobReceiptChargeDTO> JobReceiptCharges);
        Task<bool> DeleteCharges(Guid Job_Receipt_Id);
      //  Task<decimal> GetJobReceiptAmount(int supplierId);
        #endregion

    }
}
