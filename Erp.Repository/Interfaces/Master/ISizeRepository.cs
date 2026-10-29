using Erp.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Repository.Interfaces.Master
{
     public interface ISizeRepository
     {
         Task<IEnumerable<SizeDTO>> GetSizes();
         Task<SizeDTO> GetSize(Guid Size_Id);
         Task<Guid> CreateOrUpdateSize(SizeDTO Size);
         Task<bool> DeleteSize(Guid Size_Id);
     }
}
