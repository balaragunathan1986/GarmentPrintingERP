using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Master;
using Erp.Services.Interfaces.Master;

namespace Erp.Services.Implementation.Master
{
    public class StyleService : IStyleService
    {
        private readonly IStyleRepository _styleRepository;

        public StyleService(IStyleRepository styleRepository)
        {
            _styleRepository = styleRepository;
        }

        public async Task<List<StyleDTO>> GetStyles()
        {
            return await _styleRepository.GetStyles();
        }

        public async Task<StyleDTO> GetStyle(Guid StyleId)
        {
            return await _styleRepository.GetStyle(StyleId);
        }

        public async Task<StyleDTO> Save(StyleDTO style)
        {
            style.StyleId = Guid.NewGuid();
            return await _styleRepository.Save(style);
        }

        public async Task<StyleDTO> Update(StyleDTO style)
        {
            return await _styleRepository.Update(style);
        }

        public async Task<bool> Delete(Guid StyleId, string? DeletedBy)
        {
            return await _styleRepository.Delete(StyleId, DeletedBy);
        }
    }
}
