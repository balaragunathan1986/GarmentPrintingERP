namespace Erp.Domain.Model.Production
{
    public class InvalidInvoiceOutwardException : Exception
    {
        public InvalidInvoiceOutwardException(string message) : base(message) { }
    }
}
