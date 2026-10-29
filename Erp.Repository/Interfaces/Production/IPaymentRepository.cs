using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Repository.Interfaces.Production
{
    public interface IPaymentRepository
    {
        Task<string> GenerateNumber();
        Task<List<PaymentMasterDTO>> GetPayments();
        Task<PaymentMasterDTO?> GetPayment(Guid paymentId);
        Task<List<PaymentInvoiceOptionDTO>> GetAvailableInvoices(Guid supplierId, Guid? paymentId);
        Task<PaymentMasterDTO> Save(PaymentMasterDTO payment);
        Task<PaymentMasterDTO> Update(PaymentMasterDTO payment);
        Task<bool> Delete(Guid paymentId, int? deletedBy);
    }
}
