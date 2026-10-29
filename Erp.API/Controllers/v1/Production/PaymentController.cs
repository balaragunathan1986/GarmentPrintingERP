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
    public class PaymentController : ControllerBase
    {
        private static readonly string[] PaymentStatuses = ["Pending", "Partially Paid", "Paid", "Cancelled"];
        private static readonly string[] ApprovalStatuses = ["Pending", "Approved", "Rejected", "Cancelled"];
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _paymentService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Payment number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to generate payment number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var payments = await _paymentService.GetPayments();
                return Ok(ApiResponse<object>.SuccessResponse(payments, "Payment list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch payment list."));
            }
        }

        [HttpGet("AvailableInvoices")]
        public async Task<IActionResult> GetAvailableInvoices(
            [FromQuery] Guid supplierId,
            [FromQuery] Guid? paymentId)
        {
            if (supplierId == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "supplierId is required."));
            }

            try
            {
                var invoices = await _paymentService.GetAvailableInvoices(supplierId, paymentId);
                return Ok(ApiResponse<object>.SuccessResponse(invoices, "Outstanding invoices fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching outstanding invoices for supplier {SupplierId}.", supplierId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch outstanding invoices."));
            }
        }

        [HttpGet("{paymentId:guid}")]
        public async Task<IActionResult> Get(Guid paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPayment(paymentId);
                return payment == null
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Payment not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(payment, "Payment fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching payment {PaymentId}.", paymentId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch payment."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentMasterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidatePayment(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var payment = await _paymentService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(payment, "Payment saved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving payment.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save payment."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PaymentMasterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            if (model.Payment_Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Payment_Id is required."));
            }

            var validationError = ValidatePayment(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var payment = await _paymentService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(payment, "Payment updated successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Payment not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment {PaymentId}.", model.Payment_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update payment."));
            }
        }

        [HttpDelete("{paymentId:guid}")]
        public async Task<IActionResult> Delete(Guid paymentId, [FromQuery] int? deletedBy)
        {
            try
            {
                var deleted = await _paymentService.Delete(paymentId, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Payment deleted successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Payment not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment {PaymentId}.", paymentId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete payment."));
            }
        }

        private static string? ValidatePayment(PaymentMasterDTO payment)
        {
            if (string.IsNullOrWhiteSpace(payment.PaymentNo))
            {
                return "PaymentNo is required.";
            }

            if (payment.Supplier_ID == Guid.Empty)
            {
                return "Supplier_ID is required.";
            }

            if (!PaymentStatuses.Contains(payment.PaymentStatus, StringComparer.Ordinal))
            {
                return "PaymentStatus must be Pending, Partially Paid, Paid, or Cancelled.";
            }

            if (!ApprovalStatuses.Contains(payment.ApprovalStatus, StringComparer.Ordinal))
            {
                return "ApprovalStatus must be Pending, Approved, Rejected, or Cancelled.";
            }

            if (payment.Details == null)
            {
                return "Payment_Details is required.";
            }

            if (payment.Details.Any(detail => detail.PaidAmount < 0))
            {
                return "PaidAmount cannot be negative.";
            }

            if (payment.Charges.Any(charge =>
                    charge.Charge_Id == Guid.Empty
                    || charge.AddLessType is not ("+" or "-")
                    || charge.Percentage < 0
                    || charge.Amount < 0))
            {
                return "Payment charges require a charge, a valid AddLessType, and non-negative percentage and amount.";
            }

            return null;
        }
    }
}
