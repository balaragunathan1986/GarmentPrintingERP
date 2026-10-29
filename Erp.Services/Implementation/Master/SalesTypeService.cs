using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Master;
using Erp.Services.Interfaces.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Master
{
    public class SalesTypeService : ISalesTypeService
    {
        private readonly ISalesTypeRepository _salesTypeRepository;

        public SalesTypeService(ISalesTypeRepository salesTypeRepository)
        {
            _salesTypeRepository = salesTypeRepository;
        }

        public async Task<List<SalesTypeDTO>> GetSalesType()
        {
            return await _salesTypeRepository.GetSalesType();
        }

        public async Task<SalesTypeDTO> GetSalesType(Guid SalesType_Id)
        {
            return await _salesTypeRepository.GetSalesType(SalesType_Id);
        }

        public async Task<SalesTypeDTO> Save(SalesTypeDTO SalesType)
        {
            return await _salesTypeRepository.Save(SalesType);
        }

        public async Task<SalesTypeDTO> Update(SalesTypeDTO SalesType)
        {
            return await _salesTypeRepository.Update(SalesType);
        }

        public async Task<bool> Delete(Guid SalesType_Id)
        {
            return await _salesTypeRepository.Delete(SalesType_Id);
        }
    }
}
