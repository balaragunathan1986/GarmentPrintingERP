using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class MontlyExpensesService : IMontlyExpensesService
    {
        private readonly IMontlyExpensesRepository _repository;

        public MontlyExpensesService(IMontlyExpensesRepository repository)
        {
            _repository = repository;
        }

        public Task<string> GenerateDocumentNumber()
            => _repository.GenerateNumber();

        public Task<List<MontlyExpensesDTO>> GetList(string? searchText, DateTime? fromDate, DateTime? toDate)
            => _repository.GetList(searchText, fromDate, toDate);

        public Task<MontlyExpensesDTO?> Get(Guid montlyExpensesId)
            => _repository.Get(montlyExpensesId);

        public Task<MontlyExpensesModel> Save(MontlyExpensesModel model)
            => _repository.Save(model);

        public Task<MontlyExpensesModel> Update(MontlyExpensesModel model)
            => _repository.Update(model);

        public Task<bool> Delete(Guid montlyExpensesId)
            => _repository.Delete(montlyExpensesId);
    }
}
