using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model;
using Erp.Domain.Model.Purchase;
using Erp.Services.Interfaces.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Purchase
{
    [Route("api/v1/purchase/[controller]")]
    [ApiController]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(IPurchaseService purchaseService, ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpGet("generateNumber")]
        public async Task<IActionResult> GenerateNumber()
        {
            try
            {
                var number = await _purchaseService.GenerateNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Purchase number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating purchase number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to generate purchase number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? searchText,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "fromDate must be before or equal to toDate."));
            }

            try
            {
                var purchases = await _purchaseService.GetPurchases(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(purchases, "Purchase list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch purchase list."));
            }
        }

        [HttpGet("{purchaseId:guid}")]
        public async Task<IActionResult> Get(Guid purchaseId)
        {
            try
            {
                var purchase = await _purchaseService.GetPurchase(purchaseId);
                return purchase == null
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Purchase not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(purchase, "Purchase fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase {PurchaseId}.", purchaseId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch purchase."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidatePurchase(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            model.Purchase_Details ??= new();
            model.Purchase_Charges ??= new();

            try
            {
                var purchase = await _purchaseService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(purchase, "Purchase saved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save purchase."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            if (model.Purchase_Master?.Purchase_Id == Guid.Empty)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Purchase_Id is required."));
            }

            var validationError = ValidatePurchase(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            model.Purchase_Details ??= new();
            model.Purchase_Charges ??= new();

            try
            {
                var purchase = await _purchaseService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(purchase, "Purchase updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Purchase not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase {PurchaseId}.", model.Purchase_Master!.Purchase_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update purchase."));
            }
        }

        [HttpDelete("{purchaseId:guid}")]
        public async Task<IActionResult> Delete(Guid purchaseId, [FromQuery] string? deletedBy)
        {
            try
            {
                var deleted = await _purchaseService.Delete(purchaseId, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Purchase deleted successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Purchase not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase {PurchaseId}.", purchaseId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete purchase."));
            }
        }

        private static string? ValidatePurchase(PurchaseViewModel purchase)
        {
            var master = purchase.Purchase_Master;
            if (master == null)
            {
                return "Purchase_Master is required.";
            }

            if (string.IsNullOrWhiteSpace(master.Purchase_No))
            {
                return "Purchase_No is required.";
            }

            if (string.IsNullOrWhiteSpace(master.CreatedBy))
            {
                return "CreatedBy is required.";
            }

            if (master.Company_Id == Guid.Empty || master.Supplier_Id == Guid.Empty)
            {
                return "Company_Id and Supplier_Id are required.";
            }

            if (master.Sub_Total < 0 || master.Discount_Amount < 0 || master.Tax_Amount < 0
                || master.Other_Charges < 0 || master.Net_Amount < 0)
            {
                return "Purchase amounts cannot be negative.";
            }

            if (master.Tax_Percent is < 0 or > 100)
            {
                return "Tax_Percent must be between 0 and 100.";
            }

            if (purchase.Purchase_Details?.Any(detail =>
                    detail.Item_Id == Guid.Empty
                    || detail.Quantity < 0
                    || detail.Received_Qty < 0
                    || detail.Rate < 0
                    || detail.Discount_Amount < 0
                    || detail.Tax_Amount < 0
                    || detail.Amount < 0
                    || detail.Tax_Percent is < 0 or > 100) == true)
            {
                return "Purchase details require an item and non-negative quantities and amounts; tax must be between 0 and 100.";
            }

            if (purchase.Purchase_Charges?.Any(charge =>
                    charge.Charge_Id == Guid.Empty
                    || charge.AddLessType is not ("+" or "-")
                    || charge.Percentage < 0
                    || charge.Amount < 0) == true)
            {
                return "Purchase charges require a valid Charge_Id and AddLessType, and non-negative percentage and amount.";
            }

            return null;
        }
    }
}
