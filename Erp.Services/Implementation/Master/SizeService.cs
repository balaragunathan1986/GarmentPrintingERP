using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Master;
using Erp.Services.Interfaces.Master;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Master
{
     public class SizeService : ISizeService
     {
         private readonly ISizeRepository _sizeRepository;
         private readonly ILogger<SizeService> _logger;
         public SizeService(ISizeRepository sizeRepository, ILogger<SizeService> logger)
         {
             _sizeRepository = sizeRepository;
             _logger = logger;
         }
         public async Task<Guid> CreateOrUpdateSize(SizeDTO Size)
         {
             try
             {
                 _logger.LogInformation("Creating or updating size with ID: {SizeId}", Size.Size_Id);
                 return await _sizeRepository.CreateOrUpdateSize(Size);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error in CreateOrUpdateSize");
                 throw;
             }
         }

         public async Task<bool> DeleteSize(Guid Size_Id)
         {
             try
             {
                 _logger.LogInformation("Deleting size with ID: {SizeId}", Size_Id);
                 return await _sizeRepository.DeleteSize(Size_Id);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error in DeleteSize");
                 throw;
             }
         }

         public async Task<IEnumerable<SizeDTO>> GetSizes()
         {
             try
             {
                return await _sizeRepository.GetSizes();
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error in GetSizes");
                 throw;
             }
         }

         public async Task<SizeDTO> GetSize(Guid Size_Id)
         {
             try
             {
                return await _sizeRepository.GetSize(Size_Id);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error in GetSize");
                 throw;
             }
         
         }
     }
}
