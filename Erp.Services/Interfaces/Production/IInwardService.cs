using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Interfaces.Production
{
    public interface IInwardService
    {
        Task<string> GenerateNumber();
        Task<Guid> CreateOrUpdate(InwardViewModel inward);
        Task<bool> DeleteInward(Guid Inward_Id);
        Task<InwardViewModel> GetInward(Guid Inward_Id);
        Task<IEnumerable<object>> GetInward();
        Task<IEnumerable<InwardStockDTO>> GetAvailableStockDetails();
        Task<IEnumerable<InwardStockDTO>> GetStockReport();
    }
}
