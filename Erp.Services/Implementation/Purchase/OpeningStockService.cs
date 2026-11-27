using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Repository.Interfaces.Purchase;
using Erp.Services.Interfaces.Purchase;

namespace Erp.Services.Implementation.Purchase
{
    public class OpeningStockService : IOpeningStockService
    {
        private readonly IOpeningStockRepository _openingStockRepository;

        public OpeningStockService(IOpeningStockRepository openingStockRepository)
        {
            _openingStockRepository = openingStockRepository;
        }

        public Task<string> GenerateNumber() => _openingStockRepository.GenerateNumber();

        public Task<List<OpeningStockViewDTO>> GetList() => _openingStockRepository.GetList();

        public Task<OpeningStockViewDTO?> Get(string openingStockNumber)
            => _openingStockRepository.Get(openingStockNumber);

        public Task<OpeningStockViewDTO> Save(OpeningStockSaveRequestDTO request)
            => _openingStockRepository.Save(request);

        public Task<OpeningStockViewDTO> Update(OpeningStockSaveRequestDTO request)
            => _openingStockRepository.Update(request);

        public Task<bool> Delete(string openingStockNumber, string? deletedBy)
            => _openingStockRepository.Delete(openingStockNumber, deletedBy);
    }
}
