using Erp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Interfaces.Master
{
    public interface IPaymentTermsRepository
    {
        Task<List<PaymentTermsDTO>> GetPaymentTerms();
        Task<PaymentTermsDTO> GetPaymentTerms(Guid PaymentTerms_Id);
        Task<PaymentTermsDTO> Save(PaymentTermsDTO PayTerms);
        Task<PaymentTermsDTO> Update(PaymentTermsDTO PayTerms);
        Task<bool> Delete(Guid PaymentTerms_Id);
    }
}
