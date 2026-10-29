using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Repository.Interfaces.Production
{
    public interface IInvoiceRepository
    {
        Task<string> GenerateNumber();
        Task<List<InvoiceListDTO>> GetInvoices(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<InvoiceMasterDTO?> GetInvoice(Guid invoiceId);
        Task<InvoiceMasterDTO> Save(InvoiceMasterDTO invoice);
        Task<InvoiceMasterDTO> Update(InvoiceMasterDTO invoice);
        Task<bool> Delete(Guid invoiceId, int? deletedBy);
    }
}
