using Erp.Domain.DTO;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Master;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Master
{
    [Route("api/v1/masters/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemController(IItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var items = await _itemService.GetItems();
                return Ok(ApiResponse<object>.SuccessResponse(items, "Items are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch items."));
            }
        }

        [HttpGet("{Item_Id}")]
        public async Task<IActionResult> Get(Guid Item_Id)
        {
            try
            {
                var item = await _itemService.GetItem(Item_Id);
                return Ok(ApiResponse<object>.SuccessResponse(item, "Item is fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch item."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ItemDTO item)
        {
            try
            {
                var savedItem = await _itemService.Save(item);
                return Ok(ApiResponse<object>.SuccessResponse(savedItem, "Item is saved successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to save item."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ItemDTO item)
        {
            try
            {
                var updatedItem = await _itemService.Update(item);
                return Ok(ApiResponse<object>.SuccessResponse(updatedItem, "Item is updated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to update item."));
            }
        }

        [HttpDelete("{Item_Id}")]
        public async Task<IActionResult> Delete(Guid Item_Id, [FromQuery] string? DeletedBy)
        {
            try
            {
                var deleted = await _itemService.Delete(Item_Id, DeletedBy);
                return deleted
                    ? Ok(ApiResponse<string>.SuccessResponse("", "Item is deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse("Item was not found.", "Failed to delete item."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to delete item."));
            }
        }
    }
}