using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Setting;
using Erp.Services.Interfaces.Setting;

namespace Erp.Services.Implementation.Setting
{
    public class GeneralSettingsService : IGeneralSettingsService
    {
        private readonly IGeneralSettingsRepository _repository;

        public GeneralSettingsService(IGeneralSettingsRepository repository)
        {
            _repository = repository;
        }

        public Task<List<GeneralSettingsDTO>> GetGeneralSettings() => _repository.GetGeneralSettings();

        public Task<GeneralSettingsDTO?> GetGeneralSettings(int id) => _repository.GetGeneralSettings(id);

        public Task<GeneralSettingsDTO> Save(GeneralSettingsDTO settings) => _repository.Save(settings);

        public Task<GeneralSettingsDTO?> Update(GeneralSettingsDTO settings) => _repository.Update(settings);

        public Task<bool> Delete(int id) => _repository.Delete(id);
    }
}
