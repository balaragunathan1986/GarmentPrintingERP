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
    public class BillingReceiptController : ControllerBase
    {
        private readonly IBillingReceiptService _billingReceiptService;
        private readonly ILogger<BillingReceiptController> _logger;

        public BillingReceiptController(IBillingReceiptService billingReceiptService, ILogger<BillingReceiptController> logger)
        {
            _billingReceiptService = billingReceiptService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _billingReceiptService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Billing receipt number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate billing receipt number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _billingReceiptService.GetBillingReceiptList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Billing receipt list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing receipt list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch billing receipt list."));
            }
        }

        [HttpGet("{Billing_Receipt_Id}")]
        public async Task<IActionResult> Get(Guid Billing_Receipt_Id)
        {
            try
            {
                var receipt = await _billingReceiptService.GetBillingReceipt(Billing_Receipt_Id);
                return Ok(ApiResponse<object>.SuccessResponse(receipt, "Billing receipt fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing receipt {Id}.", Billing_Receipt_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch billing receipt."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BillingReceiptModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Billing_Receipt_Id == Guid.Empty)
                    model.Billing_Receipt_Id = Guid.NewGuid();

                var saved = await _billingReceiptService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Billing receipt saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving billing receipt.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save billing receipt."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] BillingReceiptModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _billingReceiptService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Billing receipt updated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating billing receipt {Id}.", model.Billing_Receipt_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update billing receipt."));
            }
        }

        [HttpDelete("{Billing_Receipt_Id}")]
        public async Task<IActionResult> Delete(Guid Billing_Receipt_Id)
        {
            try
            {
                var deleted = await _billingReceiptService.Delete(Billing_Receipt_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Billing receipt deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting billing receipt {Id}.", Billing_Receipt_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete billing receipt."));
            }
        }
    }
}