using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Purchase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Purchase
{
    [Route("api/v1/purchase/[controller]")]
    [ApiController]
    [Authorize]
    public class OpeningStockController : ControllerBase
    {
        private readonly IOpeningStockService _openingStockService;
        private readonly ILogger<OpeningStockController> _logger;

        public OpeningStockController(
            IOpeningStockService openingStockService,
            ILogger<OpeningStockController> logger)
        {
            _openingStockService = openingStockService;
            _logger = logger;
        }

        [HttpGet("generateNumber")]
        public async Task<IActionResult> GenerateNumber()
        {
            try
            {
                var number = await _openingStockService.GenerateNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Opening stock number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating opening stock number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to generate opening stock number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var openingStocks = await _openingStockService.GetList();
                return Ok(ApiResponse<object>.SuccessResponse(openingStocks, "Opening stock list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching opening stock list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch opening stock list."));
            }
        }

        [HttpGet("{openingStockNumber}")]
        public async Task<IActionResult> Get(string openingStockNumber)
        {
            try
            {
                var openingStock = await _openingStockService.Get(openingStockNumber);
                return openingStock == null
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Opening stock not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(openingStock, "Opening stock fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching opening stock {OpeningStockNumber}.", openingStockNumber);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch opening stock."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OpeningStockSaveRequestDTO request)
        {
            var validationError = ValidateRequest(request);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var openingStock = await _openingStockService.Save(request);
                return Ok(ApiResponse<object>.SuccessResponse(openingStock, "Opening stock saved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving opening stock {OpeningStockNumber}.", request.OpeningStock_Number);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save opening stock."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] OpeningStockSaveRequestDTO request)
        {
            var validationError = ValidateRequest(request);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var openingStock = await _openingStockService.Update(request);
                return Ok(ApiResponse<object>.SuccessResponse(openingStock, "Opening stock updated successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating opening stock {OpeningStockNumber}.", request.OpeningStock_Number);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update opening stock."));
            }
        }

        [HttpDelete("{openingStockNumber}")]
        public async Task<IActionResult> Delete(string openingStockNumber, [FromQuery] string? deletedBy)
        {
            try
            {
                var deleted = await _openingStockService.Delete(openingStockNumber, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Opening stock deleted successfully."))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Opening stock not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting opening stock {OpeningStockNumber}.", openingStockNumber);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete opening stock."));
            }
        }

        private static string? ValidateRequest(OpeningStockSaveRequestDTO request)
        {
            if (request == null)
            {
                return "Invalid request data.";
            }

            if (string.IsNullOrWhiteSpace(request.OpeningStock_Number)
                || request.OpeningStock_Number.Length > 15)
            {
                return "OpeningStock_Number is required and must be 15 characters or fewer.";
            }

            if (request.Company_Id <= 0 || request.Company_GUID == Guid.Empty)
            {
                return "Company_Id and Company_GUID are required.";
            }

            if (string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                return "CreatedBy is required.";
            }

            if (request.OpeningStock_Details == null || request.OpeningStock_Details.Count == 0)
            {
                return "At least one opening stock detail is required.";
            }

            if (request.OpeningStock_Details.Any(detail =>
                    detail == null
                    || detail.StockDetail == null
                    || (detail.Item_Id.GetValueOrDefault() == Guid.Empty
                        && detail.StockDetail.ItemId.GetValueOrDefault() == Guid.Empty
                        && string.IsNullOrWhiteSpace(detail.Item_Code)
                        && string.IsNullOrWhiteSpace(detail.StockDetail.ItemCode)
                        && string.IsNullOrWhiteSpace(detail.Item_Name))
                    || detail.StockDetail.Qty < 0
                    || (detail.StockDetail.Rate.HasValue && detail.StockDetail.Rate.Value < 0)))
            {
                return "Each detail requires an item ID, code, or name and non-negative quantity and rate.";
            }

            return null;
        }
    }
}
