using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Repository.Interfaces.Production;
using Erp.Services.Interfaces.Production;

namespace Erp.Services.Implementation.Production
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public Task<string> GenerateDocumentNumber() => _invoiceRepository.GenerateNumber();

        public Task<List<InvoiceListDTO>> GetInvoices(string? searchText, DateTime? fromDate, DateTime? toDate)
            => _invoiceRepository.GetInvoices(searchText, fromDate, toDate);

        public Task<InvoiceMasterDTO?> GetInvoice(Guid invoiceId)
            => _invoiceRepository.GetInvoice(invoiceId);

        public Task<InvoiceMasterDTO> Save(InvoiceMasterDTO invoice)
            => _invoiceRepository.Save(invoice);

        public Task<InvoiceMasterDTO> Update(InvoiceMasterDTO invoice)
            => _invoiceRepository.Update(invoice);

        public Task<bool> Delete(Guid invoiceId, int? deletedBy)
            => _invoiceRepository.Delete(invoiceId, deletedBy);
    }
}
