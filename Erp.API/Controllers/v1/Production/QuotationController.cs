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
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(IQuotationService quotationService, ILogger<QuotationController> logger)
        {
            _quotationService = quotationService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var quotationNumber = await _quotationService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(
                    quotationNumber,
                    "Quotation Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quotation number.");
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    ex.Message,
                    "Failed to generate quotation number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var quotations = await _quotationService.GetQuotations();
                return Ok(ApiResponse<object>.SuccessResponse(quotations, "Quotation list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotation list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch quotation list."));
            }
        }

        [HttpGet("{Quotation_Id:guid}")]
        public async Task<IActionResult> Get(Guid Quotation_Id)
        {
            try
            {
                var quotation = await _quotationService.GetQuotation(Quotation_Id);
                return quotation.Id == 0
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotation {QuotationId}.", Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch quotation."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QuotationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var quotation = await _quotationService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving quotation.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save quotation."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] QuotationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var quotation = await _quotationService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation updated successfully..!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Quotation not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quotation {QuotationId}.", model.Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update quotation."));
            }
        }

        [HttpDelete("{Quotation_Id:guid}")]
        public async Task<IActionResult> Delete(Guid Quotation_Id, [FromQuery] string? DeletedBy)
        {
            try
            {
                var deleted = await _quotationService.Delete(Quotation_Id, DeletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Quotation deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quotation {QuotationId}.", Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete quotation."));
            }
        }
    }
}