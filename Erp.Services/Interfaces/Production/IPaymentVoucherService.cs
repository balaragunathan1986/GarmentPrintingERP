using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IPaymentVoucherService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<PaymentVoucherListDTO>> GetPaymentVoucherList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<PaymentVoucherDTO> GetPaymentVoucher(Guid paymentVoucherId);
        Task<PaymentVoucherModel> Save(PaymentVoucherModel model);
        Task<PaymentVoucherModel> Update(PaymentVoucherModel model);
        Task<bool> Delete(Guid paymentVoucherId);
    }
}
