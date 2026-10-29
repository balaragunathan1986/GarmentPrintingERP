using Erp.Domain.DTO;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Master;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Master
{
    [Route("api/v1/masters/[controller]")]
    [ApiController]
    public class StyleController : ControllerBase
    {
        private readonly IStyleService _styleService;

        public StyleController(IStyleService styleService)
        {
            _styleService = styleService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var styles = await _styleService.GetStyles();
                return Ok(ApiResponse<object>.SuccessResponse(styles, "Styles are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch styles."));
            }
        }

        [HttpGet("{StyleId}")]
        public async Task<IActionResult> Get(Guid StyleId)
        {
            try
            {
                var style = await _styleService.GetStyle(StyleId);
                return Ok(ApiResponse<object>.SuccessResponse(style, "Style is fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch style."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StyleDTO style)
        {
            try
            {
                var savedStyle = await _styleService.Save(style);
                return Ok(ApiResponse<object>.SuccessResponse(savedStyle, "Style is saved successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to save style."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] StyleDTO style)
        {
            try
            {
                var updatedStyle = await _styleService.Update(style);
                return Ok(ApiResponse<object>.SuccessResponse(updatedStyle, "Style is updated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to update style."));
            }
        }

        [HttpDelete("{StyleId}")]
        public async Task<IActionResult> Delete(Guid StyleId, [FromQuery] string? DeletedBy)
        {
            try
            {
                var deleted = await _styleService.Delete(StyleId, DeletedBy);
                return deleted
                    ? Ok(ApiResponse<string>.SuccessResponse("", "Style is deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse("Style was not found.", "Failed to delete style."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to delete style."));
            }
        }
    }
}
