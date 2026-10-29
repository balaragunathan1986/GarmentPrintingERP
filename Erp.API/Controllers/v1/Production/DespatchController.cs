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
    public class DespatchController : ControllerBase
    {
        private readonly IDespatchService _despatchService;
        private readonly ILogger<DespatchController> _logger;

        public DespatchController(IDespatchService despatchService, ILogger<DespatchController> logger)
        {
            _despatchService = despatchService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _despatchService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Despatch Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate despatch number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _despatchService.GetDespatchList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Despatch list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching despatch list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch despatch list."));
            }
        }

        [HttpGet("{Production_Despatch_Id}")]
        public async Task<IActionResult> Get(Guid Production_Despatch_Id)
        {
            try
            {
                var despatch = await _despatchService.GetDespatch(Production_Despatch_Id);
                return Ok(ApiResponse<object>.SuccessResponse(despatch, "Despatch fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching despatch {Id}.", Production_Despatch_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch despatch."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DespatchModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Production_Despatch_Id == Guid.Empty)
                    model.Production_Despatch_Id = Guid.NewGuid();

                var saved = await _despatchService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Despatch saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving despatch.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save despatch."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] DespatchModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _despatchService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Despatch updated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating despatch {Id}.", model.Production_Despatch_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update despatch."));
            }
        }

        [HttpDelete("{Production_Despatch_Id}")]
        public async Task<IActionResult> Delete(Guid Production_Despatch_Id)
        {
            try
            {
                var deleted = await _despatchService.Delete(Production_Despatch_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Despatch deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting despatch {Id}.", Production_Despatch_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete despatch."));
            }
        }
    }
}