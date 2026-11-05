using Erp.Domain.DTO;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Setting;
using Erp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Setting
{
    [Route("api/v1/settings/[controller]")]
    [ApiController]
    [Authorize]
    public class GeneralSettingsController : ControllerBase
    {
        private readonly IGeneralSettingsService _service;

        public GeneralSettingsController(IGeneralSettingsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await _service.GetGeneralSettings();
                return Ok(ApiResponse<object>.SuccessResponse(list, "General settings fetched successfully..!"));
            }
            catch (Exception e)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(e.Message, "Failed to fetch general settings."));
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var item = await _service.GetGeneralSettings(id);
                if (item == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Not found", "General settings not found."));
                return Ok(ApiResponse<object>.SuccessResponse(item, "General settings fetched successfully..!"));
            }
            catch (Exception e)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(e.Message, "Failed to fetch general settings."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GeneralSettingsDTO settings)
        {
            try
            {
                var saved = await _service.Save(settings);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "General settings saved successfully..!"));
            }
            catch (Exception e)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(e.Message, "Failed to save general settings."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] GeneralSettingsDTO settings)
        {
            try
            {
                var updated = await _service.Update(settings);
                if (updated == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Not found", "General settings not found."));
                return Ok(ApiResponse<object>.SuccessResponse(updated, "General settings updated successfully..!"));
            }
            catch (Exception e)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(e.Message, "Failed to update general settings."));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.Delete(id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "General settings deleted successfully..!"));
            }
            catch (Exception e)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(e.Message, "Failed to delete general settings."));
            }
        }
    }
}
