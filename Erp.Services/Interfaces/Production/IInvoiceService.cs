using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Services.Interfaces.Production
{
    public interface IInvoiceService
    {
        Task<string> GenerateDocumentNumber();
        Task<List<InvoiceListDTO>> GetInvoices(string? searchText, DateTime? fromDate, DateTime? toDate);
        Task<InvoiceMasterDTO?> GetInvoice(Guid invoiceId);
        Task<InvoiceMasterDTO> Save(InvoiceMasterDTO invoice);
        Task<InvoiceMasterDTO> Update(InvoiceMasterDTO invoice);
        Task<bool> Delete(Guid invoiceId, int? deletedBy);
    }
}
