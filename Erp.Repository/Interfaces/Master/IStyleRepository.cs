using Erp.Domain.DTO;

namespace Erp.Repository.Interfaces.Master
{
    public interface IStyleRepository
    {
        Task<List<StyleDTO>> GetStyles();
        Task<StyleDTO> GetStyle(Guid StyleId);
        Task<StyleDTO> Save(StyleDTO style);
        Task<StyleDTO> Update(StyleDTO style);
        Task<bool> Delete(Guid StyleId, string? DeletedBy);
    }
}
