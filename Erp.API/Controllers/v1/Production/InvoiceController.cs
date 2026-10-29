using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private static readonly string[] ApprovalStatuses = ["Pending", "Approved", "Rejected", "Cancelled"];
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _invoiceService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Invoice number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to generate invoice number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? searchText,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoices(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(invoices, "Invoice list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch invoice list."));
            }
        }

        [HttpGet("{invoiceId:guid}")]
        public async Task<IActionResult> Get(Guid invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoice(invoiceId);
                return invoice == null
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Invoice not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(invoice, "Invoice fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice {InvoiceId}.", invoiceId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch invoice."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InvoiceMasterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidateInvoice(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var invoice = await _invoiceService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(invoice, "Invoice saved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving invoice.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save invoice."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] InvoiceMasterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            if (model.Invoice_Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invoice_Id is required."));
            }

            var validationError = ValidateInvoice(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var invoice = await _invoiceService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(invoice, "Invoice updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Invoice not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId}.", model.Invoice_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update invoice."));
            }
        }

        [HttpDelete("{invoiceId:guid}")]
        public async Task<IActionResult> Delete(Guid invoiceId, [FromQuery] int? deletedBy)
        {
            try
            {
                var deleted = await _invoiceService.Delete(invoiceId, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Invoice deleted successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Invoice not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}.", invoiceId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete invoice."));
            }
        }

        private static string? ValidateInvoice(InvoiceMasterDTO invoice)
        {
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNo))
            {
                return "InvoiceNo is required.";
            }

            if (!ApprovalStatuses.Contains(invoice.ApprovalStatus, StringComparer.Ordinal))
            {
                return "ApprovalStatus must be Pending, Approved, Rejected, or Cancelled.";
            }

            if (invoice.Details.Any(detail =>
                    detail.InwardQty < 0 || detail.OutwardQty < 0 || detail.Rate < 0))
            {
                return "Invoice quantities and rates cannot be negative.";
            }

            if (invoice.Charges.Any(charge =>
                    charge.AddLessType is not ("+" or "-") || charge.Percentage < 0 || charge.Amount < 0))
            {
                return "Invoice charges require a valid AddLessType and non-negative percentage and amount.";
            }

            return null;
        }
    }
}
