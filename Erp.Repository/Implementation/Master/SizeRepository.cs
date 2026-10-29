using Erp.Domain.DTO;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Master;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Erp.Repository.Implementation.Master
{
     public class SizeRepository : ISizeRepository
     {
         private readonly AppDBContext _dbContext;
         public SizeRepository(AppDBContext dBContext)
         {
            _dbContext = dBContext;
         }
         public async Task<Guid> CreateOrUpdateSize(SizeDTO Size)
         {
             var existing = await _dbContext.Size.FirstOrDefaultAsync(x => x.Size_Id == Size.Size_Id);
             if (existing == null || existing.Size_Id == Guid.Empty)
             {
                 Size.Size_Id = Guid.NewGuid();
                 await _dbContext.Set<SizeDTO>().AddAsync(Size);
                 await _dbContext.SaveChangesAsync();
                 return Size.Size_Id;
             }
             else
             {
                 existing.Size_Name = Size.Size_Name;
                 existing.Size_Description = Size.Size_Description;
                 existing.Size_Code = Size.Size_Code;
                 existing.Size_Group_Id = Size.Size_Group_Id;
                 existing.IsActive = Size.IsActive;
                 existing.IsDeleted = Size.IsDeleted;
                 existing.UpdatedAt = Size.UpdatedAt;
                 existing.UpdatedBy = Size.UpdatedBy;
                 _dbContext.Size.Update(existing);
                 await _dbContext.SaveChangesAsync();
                 return existing.Size_Id;
             }
         }

         public async Task<bool> DeleteSize(Guid Size_Id)
         {
             var existing = await _dbContext.Size.FirstOrDefaultAsync(x => x.Size_Id == Size_Id && !x.IsDeleted);
             if (existing != null)
             {
                 _dbContext.Set<SizeDTO>().Remove(existing);
                 await _dbContext.SaveChangesAsync();
                 return true;
             }
             return false;
         }

         public async Task<IEnumerable<SizeDTO>> GetSizes()
         {
            return await _dbContext.Size.Where(x => !x.IsDeleted).ToListAsync();
         }

         public async Task<SizeDTO> GetSize(Guid Size_Id)
         {
             var item = await _dbContext.Size.Where(x => !x.IsDeleted && x.Size_Id == Size_Id).FirstOrDefaultAsync();
             return item ?? new SizeDTO();
         }
     }
}
