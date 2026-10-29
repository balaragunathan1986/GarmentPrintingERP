using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Implementation.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductionBatchController : ControllerBase
    {
        private readonly IProductionBatchService _productionBatchService;
        private readonly ILogger<ProductionBatchController> _logger;

        public ProductionBatchController(IProductionBatchService productionBatchService, ILogger<ProductionBatchController> logger)
        {
            _productionBatchService = productionBatchService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var jobNumber = await _productionBatchService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(jobNumber, "Production Batch Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate job receipt number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionBatchService.GetProductionBatchList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Production Batch list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production batch list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production batch list."));
            }
        }
        [HttpGet("Batchlist")]
        public async Task<IActionResult> GetBatchList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionBatchService.GetProductionBatchCostList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Production Batch list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production batch list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production batch list."));
            }
        }

        [HttpGet("open-job-receipts")]
        public async Task<IActionResult> GetOpenJobReceipts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionBatchService.GetOpenJobReceipts(fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Open Job Receipts fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open job receipts.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch open job receipts."));
            }
        }

        [HttpGet("edit-job-receipts")]
        public async Task<IActionResult> GetEditJobReceipts(Guid Production_Batch_Id,[FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionBatchService.GetEditJobReceipts(Production_Batch_Id,fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Open Job Receipts fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open job receipts.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch open job receipts."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var productionBatchList = await _productionBatchService.GetProductionBatch();
                return Ok(ApiResponse<object>.SuccessResponse(productionBatchList, "Production Batches are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production batches.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production batches."));
            }
        }

        [HttpGet("{Production_Batch_Id}")]
        public async Task<IActionResult> Get(Guid Production_Batch_Id)
        {
            try
            {
                var productionBatch = await _productionBatchService.GetProductionBatch(Production_Batch_Id);
                return Ok(ApiResponse<object>.SuccessResponse(productionBatch, "Production Batch fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production batch {Id}.", Production_Batch_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production batch."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductionBatchModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Production_Batch_Id == Guid.Empty)
                    model.Production_Batch_Id = Guid.NewGuid();

                var saved = await _productionBatchService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Production Batch saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving production batch.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save production batch."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProductionBatchModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _productionBatchService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Production Batch updated successfully..!"));
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error updating production batch {Id}.", model.ProductionBatch?.Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update production batch."));
            }
        }

        [HttpDelete("{Production_Batch_Id}")]
        public async Task<IActionResult> Delete(Guid Production_Batch_Id)
        {
            try
            {
                var deleted = await _productionBatchService.Delete(Production_Batch_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Production Batch deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production batch {Id}.", Production_Batch_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete production batch."));
            }
        }
    }
}
