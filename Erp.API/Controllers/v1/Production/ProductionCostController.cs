using Erp.Domain.DTO.ProductionAndLogistics;
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
    public class ProductionCostController : ControllerBase
    {
        private readonly IProductionCostService _productionCostService;
        private readonly ILogger<ProductionCostController> _logger;

        public ProductionCostController(IProductionCostService productionCostService, ILogger<ProductionCostController> logger)
        {
            _productionCostService = productionCostService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
         {
            try
            {
                var number = await _productionCostService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Production Cost Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate production cost number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionCostService.GetProductionCostList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Production Cost list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production cost list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production cost list."));
            }
        }

        [HttpGet("Selectedlist")]
        public async Task<IActionResult> GetSelectedList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _productionCostService.GetProductionSelectedCostList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Production Cost list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production cost list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production cost list."));
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Get()
        //{
        //    try
        //    {
        //        var list = await _productionCostService.GetProductionCost();
        //        return Ok(ApiResponse<object>.SuccessResponse(list, "Production Costs are fetched successfully..!"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error fetching production costs.");
        //        return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production costs."));
        //    }
        //}

        [HttpGet("{Production_Cost_Id}")]
        public async Task<IActionResult> Get(Guid Production_Cost_Id)
        {
            try
            {
                var productionCost = await _productionCostService.GetProductionCost(Production_Cost_Id);
                return Ok(ApiResponse<object>.SuccessResponse(productionCost, "Production Cost fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching production cost {Id}.", Production_Cost_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch production cost."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductionCostModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Production_Cost_Id == Guid.Empty)
                    model.Production_Cost_Id = Guid.NewGuid();

                var saved = await _productionCostService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Production Cost saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving production cost.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save production cost."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProductionCostModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _productionCostService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Production Cost updated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating production cost {Id}.", model.Production_Cost_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update production cost."));
            }
        }

        [HttpDelete("{Production_Cost_Id}")]
        public async Task<IActionResult> Delete(Guid Production_Cost_Id)
        {
            try
            {
                var deleted = await _productionCostService.Delete(Production_Cost_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Production Cost deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting production cost {Id}.", Production_Cost_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete production cost."));
            }
        }
    }
}
