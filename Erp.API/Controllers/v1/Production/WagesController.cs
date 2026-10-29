using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    public class WagesController : ControllerBase
    {
        private readonly IWagesService _wagesService;

        public WagesController(IWagesService wagesService) {
            _wagesService = wagesService;
        }

        [HttpGet("GenerateNumber")]
        public async Task<IActionResult> GenerateNumber()
        {
            try
            {
                var result = await _wagesService.GenerateNumber();
                return Ok(ApiResponse<string>.SuccessResponse(result, "Wages number generated successfully."));
            }
            catch (Exception e)
            {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to generate wages number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try {
                var result = await _wagesService.GetWages();
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wages are fetched successfully."));
            }
            catch (Exception e) {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to fetch Wages."));
            }
        }

        [HttpGet("{wages_Id}")]
        public async Task<IActionResult> Get(Guid wages_Id)
        {
            try
            {
                var result = await _wagesService.GetWages(wages_Id);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wages are fetched successfully."));
            }
            catch (Exception e)
            {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to fetch Wages."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WagesModel wages)
        {
            try
            {
                var result = await _wagesService.Save(wages);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wages saved successfully."));
            }
            catch (Exception e)
            {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to save Wages."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] WagesModel wages)
        {
            try
            {
                var result = await _wagesService.Update(wages);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Wages updated successfully."));
            }
            catch (Exception e)
            {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to update Wages."));
            }
        }

        [HttpDelete("{wages_Id}")]
        public async Task<IActionResult> Delete(Guid wages_Id)
        {
            try
            {
                var result = await _wagesService.Delete(wages_Id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Wages deleted successfully."));
            }
            catch (Exception e)
            {
                return Ok(ApiResponse<string>.ErrorResponse(e.Message, "Unable to delete Wages."));
            }
        }
    }
}
