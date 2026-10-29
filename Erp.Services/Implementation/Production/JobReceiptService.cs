using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class JobReceiptService : IJobReceiptService
    {
        private readonly IJobReceiptRepository _jobReceiptRepository;
        private readonly IPaymentVoucherRepository _paymentVoucherRepository;
        public JobReceiptService(IJobReceiptRepository jobReceiptRepository, IPaymentVoucherRepository paymentVoucherRepository) {
            _jobReceiptRepository = jobReceiptRepository;
            _paymentVoucherRepository = paymentVoucherRepository;
        }
        public async Task<string> GenerateDocumentNumber()=> await _jobReceiptRepository.GenerateNumber();
        public async Task<List<JobReceiptDTO>> GetJobReceipts() => await _jobReceiptRepository.GetJobReceipt();
        public async Task<JobReceiptModel> GetJobReceipt(Guid Job_Receipt_Id)
        {
            return new JobReceiptModel { 
                JobReceipt = await _jobReceiptRepository.GetJobReceipt(Job_Receipt_Id),
                JobReceiptItems = await _jobReceiptRepository.GetJobReceiptItems(Job_Receipt_Id),
                JobReceiptCharges = await _jobReceiptRepository.GetCharges(Job_Receipt_Id)
            };
        }

        public async Task<JobReceiptModel> SaveJobReceipt(JobReceiptModel JobReceipt)
        {
            JobReceipt.JobReceipt!.Job_Receipt_Id = Guid.NewGuid();

            var receipt = await _jobReceiptRepository.Save(JobReceipt.JobReceipt);
            JobReceipt.JobReceiptItems!.ForEach(item => { item.Job_Receipt_Id = receipt.Job_Receipt_Id;});
            var item = await _jobReceiptRepository.Save(JobReceipt.JobReceiptItems!);
            JobReceipt.JobReceiptCharges!.ForEach(item => { item.Job_Receipt_Id = receipt.Job_Receipt_Id; });
            await _jobReceiptRepository.SaveCharges(JobReceipt.JobReceiptCharges!);
            return JobReceipt;
        }

        public async Task<JobReceiptModel> UpdateJobReceipt(JobReceiptModel JobReceipt)
        {
            await _jobReceiptRepository.Update(JobReceipt.JobReceipt!);
            await _jobReceiptRepository.Update(JobReceipt.JobReceiptItems!);
            await _jobReceiptRepository.UpdateCharges(JobReceipt.JobReceiptCharges!);
            return JobReceipt;
        }
        public async Task<bool> DeleteJobReceipt(Guid Job_Receipt_Id)
        {
            await _jobReceiptRepository.Delete(Job_Receipt_Id);
            await _jobReceiptRepository.DeleteItems(Job_Receipt_Id);
            await _jobReceiptRepository.DeleteCharges(Job_Receipt_Id);
            return true;
        }
        public async Task<JobReceiptItemDTO> SaveJobReceiptItem(JobReceiptItemDTO JobReceiptItem)
        {
            var jobItem = await _jobReceiptRepository.Save(JobReceiptItem);
            return jobItem;
        }
        public async Task<JobReceiptItemDTO> UpdateJobReceiptItem(JobReceiptItemDTO JobReceiptItem)
        {
            var jobItem = await _jobReceiptRepository.Update(JobReceiptItem);
            return jobItem;
        }
        public async Task<bool> DeleteJobReceiptItem(Guid Job_Receipt_Item_Id)
        { 
           var result = await _jobReceiptRepository.DeleteJobReceiptItem(Job_Receipt_Item_Id);
           return result;
        }

        public async Task<JobReceiptAdvancePaymentDTO> GetOpeningVoucerBalance(Guid supplierId)
        {
            var advancePayment = await _jobReceiptRepository.GetAdvancePayment(supplierId);

            return advancePayment;
        }
    }
}
