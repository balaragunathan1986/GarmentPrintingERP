using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Services.Interfaces.Production
{
    public interface IPaymentService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<PaymentMasterDTO>> GetPayments();
        Task<PaymentMasterDTO?> GetPayment(Guid paymentId);
        Task<List<PaymentInvoiceOptionDTO>> GetAvailableInvoices(Guid supplierId, Guid? paymentId);
        Task<PaymentMasterDTO> Save(PaymentMasterDTO payment);
        Task<PaymentMasterDTO> Update(PaymentMasterDTO payment);
        Task<bool> Delete(Guid paymentId, int? deletedBy);
    }
}
