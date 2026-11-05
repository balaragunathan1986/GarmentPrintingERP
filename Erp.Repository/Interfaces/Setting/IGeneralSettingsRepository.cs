using Erp.Domain.DTO;

namespace Erp.Repository.Interfaces.Setting
{
    public interface IGeneralSettingsRepository
    {
        Task<List<GeneralSettingsDTO>> GetGeneralSettings();
        Task<GeneralSettingsDTO?> GetGeneralSettings(int id);
        Task<GeneralSettingsDTO> Save(GeneralSettingsDTO settings);
        Task<GeneralSettingsDTO?> Update(GeneralSettingsDTO settings);
        Task<bool> Delete(int id);
    }
}
