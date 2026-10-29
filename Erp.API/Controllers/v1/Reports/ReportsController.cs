using Erp.Domain.Model;
using Erp.Services.Interfaces.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Reports
{
    [Route("api/v1/reports/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IMontlyReportService _reportService;
        private readonly IPurchaseSupplierOutstandingService _purchaseSupplierOutstandingService;
        private readonly ILogger<ReportsController> _logger;
        private readonly IReportService _commonReportService;

        public ReportsController(
            IMontlyReportService reportService,
            IPurchaseSupplierOutstandingService purchaseSupplierOutstandingService,
            ILogger<ReportsController> logger,
            IReportService commonReportService)
        {
            _reportService = reportService;
            _purchaseSupplierOutstandingService = purchaseSupplierOutstandingService;
            _logger = logger;
            _commonReportService = commonReportService;
        }

        [HttpGet("MontlyReport")]
        public async Task<IActionResult> GetMontlyReport(
            [FromQuery] string? searchText,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var report = await _reportService.GetReport(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(report, "Monthly report fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly report.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch monthly report."));
            }
        }

        [HttpGet("PurchaseSupplierOutstanding")]
        public async Task<IActionResult> GetPurchaseSupplierOutstanding(
            [FromQuery] string? searchText,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            try
            {
                var report = await _purchaseSupplierOutstandingService.GetReport(searchText, fromDate, toDate);
                return Ok(ApiResponse<object>.SuccessResponse(report, "Purchase supplier outstanding report fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase supplier outstanding report.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch purchase supplier outstanding report."));
            }
        }
        [HttpGet("summary")]
        public async Task<IActionResult> Get([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] int? companyId = null)
        {
            try
            {
                var data = await _commonReportService.GetDespatchSummary(fromDate, toDate, companyId);
                return Ok(ApiResponse<object>.SuccessResponse(data, "Despatch summary fetched successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching despatch summary.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch despatch summary."));
            }
        }
    }
}
