using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Production
{
    public class WagesService : IWagesService
    {
        private readonly IWagesRepository _wagesRepository;
        public WagesService(IWagesRepository wagesRepository) {
            _wagesRepository = wagesRepository;
        }

        public async Task<bool> Delete(Guid Wages_Id) => await _wagesRepository.Delete(Wages_Id);
        
        public async Task<string> GenerateNumber() => await _wagesRepository.GenerateNumber();

        public async Task<IList<WagesDTO>> GetWages() => await _wagesRepository.GetWages();

        public async Task<WagesModel> GetWages(Guid Wages_Id) => await _wagesRepository.GetWages(Wages_Id);

        public async Task<WagesModel> Save(WagesModel wages) => await _wagesRepository.Save(wages);

        public async Task<WagesModel> Update(WagesModel wages) => await _wagesRepository.Update(wages);
    }
}
