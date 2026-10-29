using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Interfaces.Production
{
    public interface IWagesService
    {
        Task<string> GenerateNumber();
        Task<IList<WagesDTO>> GetWages();
        Task<WagesModel> GetWages(Guid Wages_Id);
        Task<WagesModel> Save(WagesModel wages);
        Task<WagesModel> Update(WagesModel wages);
        Task<bool> Delete(Guid Wages_Id);
    }
}
