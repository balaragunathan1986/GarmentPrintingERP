using Erp.Domain.DTO;
using Erp.Repository.Data;
using Erp.Repository.Interfaces.Setting;
using Microsoft.EntityFrameworkCore;

namespace Erp.Repository.Implementation.Setting
{
    public class GeneralSettingsRepository : IGeneralSettingsRepository
    {
        private readonly AppDBContext _dbcontext;

        public GeneralSettingsRepository(AppDBContext context)
        {
            _dbcontext = context;
        }

        public async Task<List<GeneralSettingsDTO>> GetGeneralSettings()
        {
            return await _dbcontext.GeneralSettings.AsNoTracking().ToListAsync();
        }

        public async Task<GeneralSettingsDTO?> GetGeneralSettings(int id)
        {
            return await _dbcontext.GeneralSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<GeneralSettingsDTO> Save(GeneralSettingsDTO settings)
        {
            var current = await _dbcontext.GeneralSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (current != null)
            {
                current.EnableCuttingPercentage = settings.EnableCuttingPercentage;
                current.ShowQuotationRemarks = settings.ShowQuotationRemarks;
                await _dbcontext.SaveChangesAsync();
                return current;
            }

            settings.Id = 0;
            await _dbcontext.GeneralSettings.AddAsync(settings);
            await _dbcontext.SaveChangesAsync();
            return settings;
        }

        public async Task<GeneralSettingsDTO?> Update(GeneralSettingsDTO settings)
        {
            var existing = await _dbcontext.GeneralSettings.FirstOrDefaultAsync(x => x.Id == settings.Id)
                ?? await _dbcontext.GeneralSettings.OrderBy(x => x.Id).FirstOrDefaultAsync();
            if (existing == null)
                return null;

            existing.EnableCuttingPercentage = settings.EnableCuttingPercentage;
            existing.ShowQuotationRemarks = settings.ShowQuotationRemarks;
            await _dbcontext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(int id)
        {
            var existing = await _dbcontext.GeneralSettings.FirstOrDefaultAsync(x => x.Id == id);
            if (existing == null)
                return false;

            _dbcontext.GeneralSettings.Remove(existing);
            await _dbcontext.SaveChangesAsync();
            return true;
        }
    }
}
