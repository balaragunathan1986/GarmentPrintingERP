using Erp.Domain.DTO;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Master;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Master
{
    public class StyleRepository : IStyleRepository
    {
        private readonly AppDBContext _dbcontext;

        public StyleRepository(AppDBContext context)
        {
            _dbcontext = context;
        }

        public async Task<List<StyleDTO>> GetStyles()
        {
            return await _dbcontext.Style.Where(style => !style.IsDeleted && style.DeletedAt == null).ToListAsync();
        }

        public async Task<StyleDTO> GetStyle(Guid StyleId)
        {
            return await _dbcontext.Style.FirstOrDefaultAsync(style =>
                    style.StyleId == StyleId && !style.IsDeleted && style.DeletedAt == null)
                ?? new StyleDTO();
        }

        public async Task<StyleDTO> Save(StyleDTO style)
        {
            await _dbcontext.Style.AddAsync(style);
            await _dbcontext.SaveChangesAsync();
            return style;
        }

        public async Task<StyleDTO> Update(StyleDTO style)
        {
            var existingStyle = await _dbcontext.Style.FirstOrDefaultAsync(existing =>
                existing.StyleId == style.StyleId && existing.Id == style.Id &&
                !existing.IsDeleted && existing.DeletedAt == null);

            if (existingStyle == null)
            {
                return new StyleDTO();
            }

            existingStyle.Style = style.Style;
            existingStyle.Item_Id = style.Item_Id;
            existingStyle.Rate = style.Rate;
            existingStyle.IsActive = style.IsActive;
            existingStyle.UpdatedBy = style.UpdatedBy;
            existingStyle.UpdatedAt = style.UpdatedAt;

            await _dbcontext.SaveChangesAsync();
            return existingStyle;
        }

        public async Task<bool> Delete(Guid StyleId, string? DeletedBy)
        {
            var style = await _dbcontext.Style.FirstOrDefaultAsync(existing =>
                existing.StyleId == StyleId && !existing.IsDeleted && existing.DeletedAt == null);

            if (style == null)
            {
                return false;
            }

            style.DeletedBy = DeletedBy;
            style.DeletedAt = DateTime.UtcNow;
            style.IsDeleted = true;
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
