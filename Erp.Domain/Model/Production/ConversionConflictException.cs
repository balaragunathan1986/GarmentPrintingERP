namespace Erp.Domain.Model.Production
{
    public class ConversionConflictException : Exception
    {
        public ConversionConflictException(string message) : base(message) { }
    }
}
