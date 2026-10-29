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
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly IBillingPdfService _billingPdfService;
        private readonly ILogger<BillingController> _logger;

        public BillingController(IBillingService billingService, IBillingPdfService billingPdfService, ILogger<BillingController> logger)
        {
            _billingService = billingService;
            _billingPdfService = billingPdfService;
            _logger = logger;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
         {
            try
            {
                var number = await _billingService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(number, "Billing Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate billing number."));
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? searchText, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var list = await _billingService.GetBillingList(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(list, "Billing list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch billing list."));
            }
        }

        [HttpGet("{Billing_Id}")]
        public async Task<IActionResult> Get(Guid Billing_Id)
        {
            try
            {
                var billing = await _billingService.GetBilling(Billing_Id);
                return Ok(ApiResponse<object>.SuccessResponse(billing, "Billing fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing {Id}.", Billing_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch billing."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BillingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                if (model.Billing_Id == Guid.Empty)
                    model.Billing_Id = Guid.NewGuid();

                var saved = await _billingService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(saved, "Billing saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving billing.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save billing."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] BillingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));

            try
            {
                var updated = await _billingService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(updated, "Billing updated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating billing {Id}.", model.Billing_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update billing."));
            }
        }

        [HttpDelete("{Billing_Id}")]
        public async Task<IActionResult> Delete(Guid Billing_Id)
        {
            try
            {
                var deleted = await _billingService.Delete(Billing_Id);
                return Ok(ApiResponse<object>.SuccessResponse(deleted, "Billing deleted successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting billing {Id}.", Billing_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete billing."));
            }
        }
        [HttpGet("report/{Billing_Id}")]
        public async Task<IActionResult> Report(Guid Billing_Id)
        {
            try
            {
                var billing = await _billingService.GetBillingReport(Billing_Id);

                return Ok(ApiResponse<object>.SuccessResponse(
                    billing,
                    "Billing report fetched successfully..!"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching billing report {Id}.", Billing_Id);

                return StatusCode(
                    500,
                    ApiResponse<string>.ErrorResponse(
                        null,
                        "Failed to fetch billing report."
                    )
                );
            }
        }
        [HttpGet("report/{Billing_Id}/pdf")]
        public async Task<IActionResult> ReportPdf(Guid Billing_Id)
        {
            try
            {
                var billing =
                    await _billingService.GetBillingReport(Billing_Id);

                if (billing == null)
                {
                    return NotFound(
                        "Billing report not found."
                    );
                }

                var pdfBytes =
                    _billingPdfService.GeneratePdf(
                        billing
                    );

                var fileName =
                    $"Billing-{billing.BillingNumber}.pdf";

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error generating billing PDF {Id}",
                    Billing_Id
                );

                return StatusCode(
                    500,
                    "Failed to generate billing PDF."
                );
            }
        }
    }
}