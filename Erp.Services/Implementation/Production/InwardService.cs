using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Services.Implementation.Production
{
    public class InwardService : IInwardService
    {
        private readonly IInwardRepository _inwardRepository;
        private readonly ILogger<InwardService> _logger;

        public InwardService(IInwardRepository inwardRepository, ILogger<InwardService> logger)
        {
            _inwardRepository = inwardRepository;
            _logger = logger;
        }
        public async Task<string> GenerateNumber()
        {
            try
            {
                _logger.LogInformation("Generation Inward Number");
                return await _inwardRepository.GenerateNumber();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while generation inward number");
                throw;
            }
        }
        public async Task<Guid> CreateOrUpdate(InwardViewModel inward)
        {
            try
            {
                _logger.LogInformation("Creating or updating inward with ID: {InwardId}", inward.Inward_master?.Inward_Id);
                return await _inwardRepository.CreateOrUpdate(inward);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while creating or updating inward with ID: {InwardId}", inward.Inward_master?.Inward_Id);
                throw;
            }
        }

        public async Task<bool> DeleteInward(Guid Inward_Id)
        {
            try
            {
                _logger.LogInformation("Deleting inward with ID: {InwardId}", Inward_Id);
                return await _inwardRepository.DeleteInward(Inward_Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while deleting inward with ID: {InwardId}", Inward_Id);
                throw;
            }
        }

        public async Task<InwardViewModel> GetInward(Guid Inward_Id)
        {
            try
            {
                _logger.LogInformation("Retrieving inward with ID: {InwardId}", Inward_Id);
                return await _inwardRepository.GetInward(Inward_Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while retrieving inward with ID: {InwardId}", Inward_Id);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetInward()
        {
            try
            {
                _logger.LogInformation("Retrieving all inwards.");
                return await _inwardRepository.GetInward();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while retrieving all inwards.");
                throw;
            }
        }

        public async Task<IEnumerable<InwardStockDTO>> GetAvailableStockDetails()
        {
            try
            {
                _logger.LogInformation("Retrieving available stock details for inward utilization.");
                return await _inwardRepository.GetAvailableStockDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving available stock details.");
                throw;
            }
        }

        public async Task<IEnumerable<InwardStockDTO>> GetStockReport()
        {
            try
            {
                return await _inwardRepository.GetStockReport();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving stock report.");
                throw;
            }
        }
    }
}
