using Erp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Repository.Interfaces.Master
{
    public interface IColorRepository
    {
        Task<IEnumerable<ColorDTO>> GetColors();
        Task<ColorDTO> GetColors(Guid Color_Id);
        Task<Guid> CreateOrUpdateColor(ColorDTO Color);
        Task<bool> DeleteColor(Guid Color_Id);
    }
}
