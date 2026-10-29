using Erp.Domain.DTO;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Erp.API.Controllers.v1.Master
{
    [Route("api/masters/[controller]")]
    [ApiController]
    [Authorize]
    public class ColorController : ControllerBase
    {
        private readonly IColorService _colorService;

        public ColorController(IColorService colorService)
        {
            _colorService = colorService;
        }
        // GET: api/<ColorController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try 
            {
                var result = await _colorService.GetColors();
                return Ok(ApiResponse<object>.SuccessResponse(result , "Colors retrived succussfully.!"));
            }
            catch (Exception ex) {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Failed while retriving colors.!"));
            }
        }

        // GET api/<ColorController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var result = await _colorService.GetColors(id);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Color retrived succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Failed while retriving color.!"));
            }
        }

        // POST api/<ColorController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ColorDTO Color)
        {
            try
            {
                var result = await _colorService.CreateOrUpdateColor(Color);
                return Ok(ApiResponse<Guid>.SuccessResponse(result, "Color saved succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Failed while saving color.!"));
            }
        }

        // PUT api/<ColorController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ColorDTO Color)
        {
            try
            {
                var result = await _colorService.CreateOrUpdateColor(Color);
                return Ok(ApiResponse<Guid>.SuccessResponse(result, "Color updated succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Failed while updating color.!"));
            }
        }

        // DELETE api/<ColorController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {

            try
            {
                var result = await _colorService.DeleteColor(id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Color deleted succussfully.!"));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<string>.ErrorResponse(ex.Message, "Failed while deleting color.!"));
            }
        }
    }
}
