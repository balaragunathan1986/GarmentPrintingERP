using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public Task<string> GenerateDocumentNumber() => _paymentRepository.GenerateNumber();
        public Task<List<PaymentMasterDTO>> GetPayments() => _paymentRepository.GetPayments();
        public Task<PaymentMasterDTO?> GetPayment(Guid paymentId) => _paymentRepository.GetPayment(paymentId);
        public Task<List<PaymentInvoiceOptionDTO>> GetAvailableInvoices(Guid supplierId, Guid? paymentId)
            => _paymentRepository.GetAvailableInvoices(supplierId, paymentId);
        public Task<PaymentMasterDTO> Save(PaymentMasterDTO payment) => _paymentRepository.Save(payment);
        public Task<PaymentMasterDTO> Update(PaymentMasterDTO payment) => _paymentRepository.Update(payment);
        public Task<bool> Delete(Guid paymentId, int? deletedBy) => _paymentRepository.Delete(paymentId, deletedBy);
    }
}
