using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Implementation.Production
{
    public class WagesRepository : IWagesRepository
    {
        private readonly AppDBContext _dbContext;
        public WagesRepository(AppDBContext appContext) {
            _dbContext = appContext;
        }
        public async Task<string> GenerateNumber()
        {
            var WagesNumber = await _dbContext.Wages.OrderByDescending(x => x.Wages_Number).Select(itm => itm.Wages_Number).FirstOrDefaultAsync();
            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.Wages, Utility.CommonFunctions.GetTransactionNumber(WagesNumber));
        }
        public async Task<bool> Delete(Guid Wages_Id)
        {
            var wages = await _dbContext.Wages.FirstOrDefaultAsync(item => item.Wages_Id == Wages_Id && item.isDeleted == false);
            if (wages != null)
            {
                _dbContext.Wages.Remove(wages);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
                return false;
        }

        public async Task<IList<WagesDTO>> GetWages()
        {
            return await _dbContext.Wages
            .Where(item => item.isDeleted == false)
            .OrderByDescending(item => item.Id)
            .ToListAsync();
        }

        public async  Task<WagesModel> GetWages(Guid Wages_Id)
        {
           var wages =  await _dbContext.Wages.FirstOrDefaultAsync(item => item.Wages_Id == Wages_Id && item.isDeleted == false);
           var wages_charges = await _dbContext.WagesCharges.Where(item => item.Wages_Id == Wages_Id).ToListAsync();
            return new WagesModel()
            {
                WagesMaster = wages,
                WagesDetails = wages_charges
            };
        }

        public async Task<WagesModel> Save(WagesModel wages)
        {
            var validateNumber = await GenerateNumber();
            if (validateNumber != null)
            {
                if(wages.WagesMaster != null)
                    wages.WagesMaster.Wages_Number = validateNumber == wages.WagesMaster.Wages_Number ? wages.WagesMaster.Wages_Number : validateNumber;
            }

            if (wages.WagesMaster != null) {
                await _dbContext.Wages.AddAsync(wages.WagesMaster);
                if(wages.WagesDetails != null)
                    await _dbContext.WagesCharges.AddRangeAsync(wages.WagesDetails);
                await _dbContext.SaveChangesAsync();
            }
            return wages;
        }

        public async Task<WagesModel> Update(WagesModel wages)
        {
            if (wages.WagesMaster != null)
            {
                 _dbContext.Wages.Update(wages.WagesMaster);
                if (wages.WagesDetails != null)
                     _dbContext.WagesCharges.UpdateRange(wages.WagesDetails);

                await _dbContext.SaveChangesAsync();
            }
            return wages;
        }

    }
}
