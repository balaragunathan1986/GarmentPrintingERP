using Erp.Domain.DTO;

namespace Erp.Services.Interfaces.Setting
{
    public interface IGeneralSettingsService
    {
        Task<List<GeneralSettingsDTO>> GetGeneralSettings();
        Task<GeneralSettingsDTO?> GetGeneralSettings(int id);
        Task<GeneralSettingsDTO> Save(GeneralSettingsDTO settings);
        Task<GeneralSettingsDTO?> Update(GeneralSettingsDTO settings);
        Task<bool> Delete(int id);
    }
}
