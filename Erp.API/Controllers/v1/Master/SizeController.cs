using Erp.Domain.DTO;
using Erp.Services.Interfaces.Master;
using Erp.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Master
{
 [Route("api/masters/[controller]")]
 [ApiController]
 [Authorize]
 public class SizeController : ControllerBase
 {
     private readonly ISizeService _sizeService;
     public SizeController(ISizeService sizeService)
     {
     _sizeService = sizeService;
     }

     [HttpGet]
     public async Task<IActionResult> Get()
     {
         try
         {
             var result = await _sizeService.GetSizes();
             return Ok(ApiResponse<object>.SuccessResponse(result, "Sizes retrieved successfully..!"));
         }
         catch (Exception ex)
         {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch sizes."));
         }
     }

     [HttpGet("{id}")]
     public async Task<IActionResult> Get(Guid id)
     {
         try
         {
             var result = await _sizeService.GetSize(id);
             return Ok(ApiResponse<object>.SuccessResponse(result, "Size retrieved successfully..!"));
         }
         catch (Exception ex)
         {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch size."));
         }
     }

     [HttpPost]
     public async Task<IActionResult> Post([FromBody] SizeDTO size)
     {
         try
         {
             var result = await _sizeService.CreateOrUpdateSize(size);
             return Ok(ApiResponse<Guid>.SuccessResponse(result, "Size saved successfully..!"));
         }
         catch (Exception ex)
         {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to save size."));
         }
     }

     [HttpPut]
     public async Task<IActionResult> Put([FromBody] SizeDTO size)
     {
         try
         {
             var result = await _sizeService.CreateOrUpdateSize(size);
             return Ok(ApiResponse<Guid>.SuccessResponse(result, "Size updated successfully..!"));
         }
         catch (Exception ex)
         {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to update size."));
         }
     }

     [HttpDelete("{id}")]
     public async Task<IActionResult> Delete(Guid id)
     {
         try
         {
             var result = await _sizeService.DeleteSize(id);
             return Ok(ApiResponse<bool>.SuccessResponse(result, "Size deleted successfully..!"));
         }
         catch (Exception ex)
         {
             return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to delete size."));
         }
     }
 }
}
