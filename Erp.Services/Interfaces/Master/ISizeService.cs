using Erp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Services.Interfaces.Master
{
     public interface ISizeService
     {
         Task<IEnumerable<SizeDTO>> GetSizes();
         Task<SizeDTO> GetSize(Guid Size_Id);
         Task<Guid> CreateOrUpdateSize(SizeDTO Size);
         Task<bool> DeleteSize(Guid Size_Id);
     }
}
