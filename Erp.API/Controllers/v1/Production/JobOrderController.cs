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
    public class JobOrderController : ControllerBase
    {
        private readonly IJobOrderService _jobOrderService;
        private readonly ILogger<JobOrderController> _logger;

        public JobOrderController(IJobOrderService jobOrderService, ILogger<JobOrderController> logger)
        {
            _jobOrderService = jobOrderService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var jobOrderNumber = await _jobOrderService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(
                    jobOrderNumber,
                    "Job Order Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating job order number.");
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    ex.Message,
                    "Failed to generate job order number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var jobOrders = await _jobOrderService.GetJobOrders();
                return Ok(ApiResponse<object>.SuccessResponse(jobOrders, "Job Order list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job order list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch job order list."));
            }
        }

        [HttpGet("{JobOrder_Id:guid}")]
        public async Task<IActionResult> Get(Guid JobOrder_Id)
        {
            try
            {
                var jobOrder = await _jobOrderService.GetJobOrder(JobOrder_Id);
                return jobOrder.Id == 0
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Job Order not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(jobOrder, "Job Order fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job order {JobOrderId}.", JobOrder_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch job order."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobOrderModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var jobOrder = await _jobOrderService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(jobOrder, "Job Order saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving job order.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save job order."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] JobOrderModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var jobOrder = await _jobOrderService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(jobOrder, "Job Order updated successfully..!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Job Order not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job order {JobOrderId}.", model.JobOrder_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update job order."));
            }
        }

        [HttpDelete("{JobOrder_Id:guid}")]
        public async Task<IActionResult> Delete(Guid JobOrder_Id, [FromQuery] string? DeletedBy)
        {
            try
            {
                var deleted = await _jobOrderService.Delete(JobOrder_Id, DeletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Job Order deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Job Order not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting job order {JobOrderId}.", JobOrder_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete job order."));
            }
        }
    }
}
