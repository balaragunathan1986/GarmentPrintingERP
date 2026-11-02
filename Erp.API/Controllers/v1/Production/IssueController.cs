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
    public class IssueController : ControllerBase
    {
        private readonly IIssueService _issueService;
        private readonly ILogger<IssueController> _logger;

        public IssueController(IIssueService issueService, ILogger<IssueController> logger)
        {
            _issueService = issueService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var issueNumber = await _issueService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(
                    issueNumber,
                    "Issue Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating issue number.");
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    ex.Message,
                    "Failed to generate issue number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var issues = await _issueService.GetIssues();
                return Ok(ApiResponse<object>.SuccessResponse(issues, "Issue list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching issue list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch issue list."));
            }
        }

        [HttpGet("{Issue_Id:guid}")]
        public async Task<IActionResult> Get(Guid Issue_Id)
        {
            try
            {
                var issue = await _issueService.GetIssue(Issue_Id);
                return issue.Id == 0
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Issue not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(issue, "Issue fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching issue {IssueId}.", Issue_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch issue."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] IssueModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var issue = await _issueService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(issue, "Issue saved successfully..!"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid request data."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving issue.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save issue."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] IssueModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            try
            {
                var issue = await _issueService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(issue, "Issue updated successfully..!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Issue not found."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Invalid request data."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating issue {IssueId}.", model.Issue_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update issue."));
            }
        }

        [HttpDelete("{Issue_Id:guid}")]
        public async Task<IActionResult> Delete(Guid Issue_Id, [FromQuery] string? DeletedBy)
        {
            try
            {
                var deleted = await _issueService.Delete(Issue_Id, DeletedBy);
                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Issue deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Issue not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting issue {IssueId}.", Issue_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete issue."));
            }
        }
    }
}
