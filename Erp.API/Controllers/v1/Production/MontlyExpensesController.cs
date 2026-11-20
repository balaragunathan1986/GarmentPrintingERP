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
    public class MontlyExpensesController : ControllerBase
    {
        private readonly IMontlyExpensesService _service;
        private readonly ILogger<MontlyExpensesController> _logger;

        public MontlyExpensesController(IMontlyExpensesService service, ILogger<MontlyExpensesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var number = await _service.GenerateDocumentNumber();
                return Ok(ApiResponse<string>.SuccessResponse(number, "Monthly Expenses number generated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Monthly Expenses number.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to generate Monthly Expenses number."));
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
                var list = await _service.GetList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Monthly Expenses list fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Monthly Expenses list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch Monthly Expenses list."));
            }
        }

        [HttpGet("{montlyExpensesId:guid}")]
        public async Task<IActionResult> Get(Guid montlyExpensesId)
        {
            try
            {
                var expense = await _service.Get(montlyExpensesId);
                if (expense == null)
                    return NotFound(ApiResponse<string>.ErrorResponse(null, "Monthly Expenses not found."));

                return Ok(ApiResponse<object>.SuccessResponse(expense, "Monthly Expenses fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Monthly Expenses {Id}.", montlyExpensesId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch Monthly Expenses."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MontlyExpensesModel model)
        {
            try
            {
                var saved = await _service.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Monthly Expenses saved successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid Monthly Expenses data."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Monthly Expenses.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save Monthly Expenses."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] MontlyExpensesModel model)
        {
            try
            {
                var updated = await _service.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Monthly Expenses updated successfully."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid Monthly Expenses data."));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(null, "Monthly Expenses not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Monthly Expenses {Id}.", model.MontlyExpenses_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update Monthly Expenses."));
            }
        }

        [HttpDelete("{montlyExpensesId:guid}")]
        public async Task<IActionResult> Delete(Guid montlyExpensesId)
        {
            try
            {
                var deleted = await _service.Delete(montlyExpensesId);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.SuccessResponse(false, "Monthly Expenses not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Monthly Expenses deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Monthly Expenses {Id}.", montlyExpensesId);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete Monthly Expenses."));
            }
        }
    }
}
