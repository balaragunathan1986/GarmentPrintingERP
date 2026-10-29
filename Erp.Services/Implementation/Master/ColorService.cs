using Erp.Domain.DTO;
using Erp.Repository.Interfaces.Master;
using Erp.Services.Interfaces.Master;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Master
{
    public class ColorService : IColorService
    {
        private readonly IColorRepository _colorRepository;
        private readonly ILogger<ColorService> _logger;
        public ColorService(IColorRepository colorRepository, ILogger<ColorService> logger)
        {
            _colorRepository = colorRepository;
            _logger = logger;
        }
        public async Task<Guid> CreateOrUpdateColor(ColorDTO Color)
        {
            try
            {
                _logger.LogInformation("Creating or updating color with ID: {ColorId}", Color.Color_Id);
                return await _colorRepository.CreateOrUpdateColor(Color);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating or updating the color.");
                throw;
            }
        }

        public async Task<bool> DeleteColor(Guid Color_Id)
        {
            try 
            {
                _logger.LogInformation("Deleting color with ID: {ColorId}", Color_Id);
                return await _colorRepository.DeleteColor(Color_Id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the color with ID: {ColorId}", Color_Id);
                throw;
            }   
        }

        public async Task<IEnumerable<ColorDTO>> GetColors()
        {
            try 
            { 
                _logger.LogInformation("Retrieving all colors.");
                var colors = await _colorRepository.GetColors();
                return colors ?? new List<ColorDTO>();

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving colors.");
                throw;
            }
        }

        public Task<ColorDTO> GetColors(Guid Color_Id)
        {
            try
            {
                _logger.LogInformation("Retrieving color with ID: {ColorId}", Color_Id);
                return _colorRepository.GetColors(Color_Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the color with ID: {ColorId}", Color_Id);
                throw;
            }
        }
    }
}
