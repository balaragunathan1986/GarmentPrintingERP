using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class PaymentVoucherService : IPaymentVoucherService
    {
        private readonly IPaymentVoucherRepository _paymentVoucherRepository;

        public PaymentVoucherService(IPaymentVoucherRepository paymentVoucherRepository)
        {
            _paymentVoucherRepository = paymentVoucherRepository;
        }

        public async Task<string> GenerateDocumentNumber()
            => await _paymentVoucherRepository.GenerateNumber();

        public async Task<List<PaymentVoucherListDTO>> GetPaymentVoucherList(string? searchText, DateTime? fromDate, DateTime? toDate)
            => await _paymentVoucherRepository.GetPaymentVoucherList(searchText, fromDate, toDate);

        public async Task<PaymentVoucherDTO> GetPaymentVoucher(Guid paymentVoucherId)
            => await _paymentVoucherRepository.GetPaymentVoucher(paymentVoucherId);

        public async Task<PaymentVoucherModel> Save(PaymentVoucherModel model)
            => await _paymentVoucherRepository.Save(model);

        public async Task<PaymentVoucherModel> Update(PaymentVoucherModel model)
            => await _paymentVoucherRepository.Update(model);

        public async Task<bool> Delete(Guid paymentVoucherId)
            => await _paymentVoucherRepository.Delete(paymentVoucherId);
    }
}
