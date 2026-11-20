using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model.Production;

namespace Erp.Services.Interfaces.Production
{
    public interface IMontlyExpensesService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<MontlyExpensesDTO>> GetList(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<MontlyExpensesDTO?> Get(Guid montlyExpensesId);
        Task<MontlyExpensesModel> Save(MontlyExpensesModel model);
        Task<MontlyExpensesModel> Update(MontlyExpensesModel model);
        Task<bool> Delete(Guid montlyExpensesId);
    }
}
