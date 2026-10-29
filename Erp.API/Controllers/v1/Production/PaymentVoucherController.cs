using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentVoucherController : ControllerBase
    {
        private readonly IPaymentVoucherService _paymentVoucherService;
        private readonly ILogger<PaymentVoucherController> _logger;

        public PaymentVoucherController(IPaymentVoucherService paymentVoucherService, ILogger<PaymentVoucherController> logger)
        {
            _paymentVoucherService = paymentVoucherService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _paymentVoucherService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Payment Voucher Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate Payment Voucher  number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _paymentVoucherService.GetPaymentVoucherList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Payment Voucher list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Payment Voucher list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch Payment Voucher list."));
            }
        }

        [HttpGet("{Payment_Voucher_Id}")]
        public async Task<IActionResult> Get(Guid Payment_Voucher_Id)
        {
            try
            {
                var paymentVoucher = await _paymentVoucherService.GetPaymentVoucher(Payment_Voucher_Id);
                return Ok(ApiResponse<object>.SuccessResponse(paymentVoucher, "Payment Voucher fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Payment Voucher {Id}.", Payment_Voucher_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch Payment Voucher."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentVoucherModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Payment_Voucher_Id == Guid.Empty)
                    model.Payment_Voucher_Id = Guid.NewGuid();

                var saved = await _paymentVoucherService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Payment Voucher saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Payment Voucher.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save Payment Voucher."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PaymentVoucherModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _paymentVoucherService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Payment Voucher updated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Payment Voucher {Id}.", model.Payment_Voucher_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update Payment Voucher."));
            }
        }

        [HttpDelete("{Payment_Voucher_Id}")]
        public async Task<IActionResult> Delete(Guid Payment_Voucher_Id)
        {
            try
            {
                var deleted = await _paymentVoucherService.Delete(Payment_Voucher_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Payment Voucher deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Payment Voucher {Id}.", Payment_Voucher_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete Payment Voucher."));
            }
        }
    }
}