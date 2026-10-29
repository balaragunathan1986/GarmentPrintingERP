using Erp.Domain.DTO;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Master;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Implementation.Master
{
    public class ColorRepository : IColorRepository
    {
        private readonly AppDBContext _dbContext;
        public ColorRepository(AppDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        public async Task<Guid> CreateOrUpdateColor(ColorDTO Color)
        {
            var existingColor = await _dbContext.Color.FindAsync(Color.Id);
            if (existingColor == null)
            {
                Color.Color_Id = Guid.NewGuid();
                await _dbContext.Color.AddAsync(Color);
                await _dbContext.SaveChangesAsync();
                return Color.Color_Id;
            }
            else { 
                existingColor.Color_Name = Color.Color_Name;
                existingColor.Color_Description = Color.Color_Description;
                existingColor.Color_Code = Color.Color_Code;
                existingColor.Pantone_Code = Color.Pantone_Code;
                existingColor.IsActive = Color.IsActive;
                existingColor.IsDeleted = Color.IsDeleted;
                _dbContext.Color.Update(existingColor);
                await _dbContext.SaveChangesAsync();
                return existingColor.Color_Id;

            }
        }

        public async Task<bool> DeleteColor(Guid Color_Id)
        {
           var exsistingColor = await  _dbContext.Color.Where(x=>x.Color_Id == Color_Id).FirstOrDefaultAsync();
            if (exsistingColor != null)
            {
                 _dbContext.Color.Remove(exsistingColor);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<ColorDTO>> GetColors()
        {
            var colors = await _dbContext.Color.Where(_=>!_.IsDeleted).ToListAsync();
            return colors;
        }

        public async Task<ColorDTO> GetColors(Guid Color_Id)
        {
            var colors = await _dbContext.Color.Where(_ => !_.IsDeleted && _.Color_Id == Color_Id).FirstOrDefaultAsync();
            return colors ?? new ColorDTO();
        }
    }
}
