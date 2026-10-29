using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Repository.Interfaces.Production
{
    public interface IPaymentVoucherRepository
    {
        Task<string> GenerateNumber();
        Task<List<PaymentVoucherListDTO>> GetPaymentVoucherList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<PaymentVoucherDTO> GetPaymentVoucher(Guid paymentVoucherId);
        Task<PaymentVoucherModel> Save(PaymentVoucherModel model);
        Task<PaymentVoucherModel> Update(PaymentVoucherModel model);
        Task<bool> Delete(Guid paymentVoucherId);
    }
}
