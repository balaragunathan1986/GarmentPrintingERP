using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    public class InwardController : ControllerBase
    {
        private readonly IInwardService _inwardService;

        public InwardController(IInwardService inwardService)
        {
            _inwardService = inwardService;
        }

        [HttpGet("generateNumber")]
        public async Task<IActionResult> GenerateNumber()
        {
            try
            {
                var result = await _inwardService.GenerateNumber();
                return Ok(ApiResponse<string>.SuccessResponse(result, "Inward number generated successfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward number generation failed.!"));
            }
        }

        // GET: api/<InwardController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try { 
                var result = await _inwardService.GetInward();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Inward retived succussfully.!"));
            } 
            catch (Exception ex) 
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward retived failed.!"));
            }
        }

        // GET api/<InwardController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _inwardService.GetInward(id);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Inward retived succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward retived failed.!"));
            }
        }

        [HttpGet("StockDetails")]
        public async Task<IActionResult> GetStockDetails()
        {
            try
            {
                var result = await _inwardService.GetAvailableStockDetails();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Available stock details retrieved successfully."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Available stock details retrieval failed."));
            }
        }

        [HttpGet("StockReport")]
        public async Task<IActionResult> GetStockReport()
        {
            try
            {
                var result = await _inwardService.GetStockReport();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Stock report retrieved successfully."));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Stock report retrieval failed."));
            }
        }

        // POST api/<InwardController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InwardViewModel inward)
        {
            try
            {
                var result = await _inwardService.CreateOrUpdate(inward);
                return Ok(ApiResponse<Guid>.SuccessResponse(result, "Inward saved succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward saving failed.!"));
            }
        }

        // PUT api/<InwardController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] InwardViewModel inward)
        {
            try
            {
                var result = await _inwardService.CreateOrUpdate(inward);
                return Ok(ApiResponse<Guid>.SuccessResponse(result, "Inward updated succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward updating failed.!"));
            }
        }

        // DELETE api/<InwardController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _inwardService.DeleteInward(id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Inward deleted succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Inward deleting failed.!"));
            }
        }
    }
}
