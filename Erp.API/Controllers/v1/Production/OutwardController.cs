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
    public class OutwardController : ControllerBase
    {
        private readonly IOutwardService _outwardService;
        private readonly ILogger<OutwardController> _logger;

        public OutwardController(IOutwardService outwardService, ILogger<OutwardController> logger)
        {
            _outwardService = outwardService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _outwardService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Outward Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating outward number.");
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate outward number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var outwards = await _outwardService.GetOutwards();
                return Ok(ApiResponse<object>.SuccessResponse(outwards, "Outward list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching outward list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch outward list."));
            }
        }

        [HttpGet("AvailableInwards")]
        public async Task<IActionResult> GetAvailableInwards()
        {
            try
            {
                var inwards = await _outwardService.GetAvailableInwards();
                return Ok(ApiResponse<object>.SuccessResponse(inwards, "Available inwards fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching available inwards.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch available inwards."));
            }
        }

        [HttpGet("InwardItems/{inwardId:guid}")]
        public async Task<IActionResult> GetInwardItems(Guid inwardId)
        {
            try
            {
                var items = await _outwardService.GetInwardItems(inwardId);
                return Ok(ApiResponse<object>.SuccessResponse(items, "Inward items fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items for inward {InwardId}.", inwardId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch inward items."));
            }
        }

        [HttpGet("{outwardId:guid}")]
        public async Task<IActionResult> Get(Guid outwardId)
        {
            try
            {
                var outward = await _outwardService.GetOutward(outwardId);
                return outward.OutwardID == 0
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Outward not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(outward, "Outward fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching outward {OutwardId}.", outwardId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch outward."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OutwardModel model)
        {
            if (model.Details == null || model.Details.Count == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "At least one outward detail is required."));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var outward = await _outwardService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(outward, "Outward saved successfully..!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid outward quantities."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving outward.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save outward."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] OutwardModel model)
        {
            if (model.Details == null || model.Details.Count == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "At least one outward detail is required."));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var outward = await _outwardService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(outward, "Outward updated successfully..!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Outward not found."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid outward quantities."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating outward {OutwardId}.", model.Outward_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update outward."));
            }
        }

        [HttpDelete("{outwardId:guid}")]
        public async Task<IActionResult> Delete(Guid outwardId, [FromQuery] string? deletedBy)
        {
            try
            {
                var deleted = await _outwardService.Delete(outwardId, deletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Outward deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Outward not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting outward {OutwardId}.", outwardId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete outward."));
            }
        }
    }
}
