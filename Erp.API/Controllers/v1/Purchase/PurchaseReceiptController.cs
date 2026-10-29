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
    public class PurchaseReceiptController : ControllerBase
    {
        private readonly IPurchaseReceiptService _purchaseReceiptService;
        private readonly ILogger<PurchaseReceiptController> _logger;

        public PurchaseReceiptController(
            IPurchaseReceiptService purchaseReceiptService,
            ILogger<PurchaseReceiptController> logger)
        {
            _purchaseReceiptService = purchaseReceiptService;
            _logger = logger;
        }

        [HttpGet("generateNumber")]
        public async Task<IActionResult> GenerateNumber()
        {
            try
            {
                var number = await _purchaseReceiptService.GenerateNumber();
                return Ok(ApiResponse<object>.SuccessResponse(
                    number, "Purchase receipt number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating purchase receipt number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to generate purchase receipt number."));
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
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    null, "fromDate must be before or equal to toDate."));
            }

            try
            {
                var receipts = await _purchaseReceiptService.GetReceipts(
                    searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(
                    receipts, "Purchase receipt list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase receipt list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to fetch purchase receipt list."));
            }
        }

        [HttpGet("purchases/{purchaseId:guid}/details")]
        public async Task<IActionResult> GetPurchaseDetails(Guid purchaseId)
        {
            try
            {
                var details = await _purchaseReceiptService.GetPurchaseDetails(purchaseId);
                return Ok(ApiResponse<object>.SuccessResponse(
                    details, "Purchase details fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching details for purchase {PurchaseId}.", purchaseId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to fetch purchase details."));
            }
        }

        [HttpGet("{receiptId:guid}")]
        public async Task<IActionResult> Get(Guid receiptId)
        {
            try
            {
                var receipt = await _purchaseReceiptService.GetReceipt(receiptId);
                return receipt == null
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Purchase receipt not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(
                        receipt, "Purchase receipt fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase receipt {ReceiptId}.", receiptId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to fetch purchase receipt."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurchaseReceiptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidateReceipt(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var receipt = await _purchaseReceiptService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(
                    receipt, "Purchase receipt saved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving purchase receipt.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to save purchase receipt."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PurchaseReceiptViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidateReceipt(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var receipt = await _purchaseReceiptService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(
                    receipt, "Purchase receipt updated successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purchase receipt.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to update purchase receipt."));
            }
        }

        [HttpDelete("{receiptId:guid}")]
        public async Task<IActionResult> Delete(Guid receiptId, [FromQuery] string? deletedBy)
        {
            try
            {
                var deleted = await _purchaseReceiptService.Delete(receiptId, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Purchase receipt deleted successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Purchase receipt not found."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase receipt {ReceiptId}.", receiptId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(
                    null, "Failed to delete purchase receipt."));
            }
        }

        private static string? ValidateReceipt(PurchaseReceiptViewModel receipt)
        {
            var master = receipt.PurchaseReceipt_Master;
            if (master == null)
            {
                return "PurchaseReceipt_Master is required.";
            }

            if (string.IsNullOrWhiteSpace(master.Receipt_No))
            {
                return "Receipt_No is required.";
            }

            if (master.Receipt_No.Length > 15)
            {
                return "Receipt_No cannot exceed 15 characters because it is stored in StockDetail.StockNo.";
            }

            if (string.IsNullOrWhiteSpace(master.CreatedBy))
            {
                return "CreatedBy is required.";
            }

            if (master.Company_Id == Guid.Empty || master.Supplier_Id == Guid.Empty)
            {
                return "Company_Id and Supplier_Id are required.";
            }

            if (receipt.PurchaseReceipt_Details == null
                || receipt.PurchaseReceipt_Details.Count == 0)
            {
                return "At least one purchase receipt detail is required.";
            }

            if (receipt.PurchaseReceipt_Details.Any(detail =>
                    detail.Purchase_Detail_Id == Guid.Empty
                    || detail.Received_Qty < 0
                    || detail.Accepted_Qty < 0
                    || detail.Rejected_Qty < 0
                    || detail.Shortage_Qty < 0
                    || detail.Rate < 0
                    || detail.Accepted_Qty + detail.Rejected_Qty > detail.Received_Qty))
            {
                return "Receipt details require a purchase detail and non-negative quantities and rate; accepted plus rejected quantity cannot exceed received quantity.";
            }

            if (receipt.PurchaseReceipt_Details
                .GroupBy(detail => detail.Purchase_Detail_Id)
                .Any(group => group.Count() > 1))
            {
                return "A purchase detail can appear only once in a receipt.";
            }

            return null;
        }
    }
}
