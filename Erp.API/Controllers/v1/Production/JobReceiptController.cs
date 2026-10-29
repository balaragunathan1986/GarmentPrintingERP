using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    [Authorize]
    public class JobReceiptController : ControllerBase
    {
        private readonly IJobReceiptService _jobReceiptService;
        public JobReceiptController(IJobReceiptService jobReceiptService) => _jobReceiptService = jobReceiptService;

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var jobNumber = await _jobReceiptService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(jobNumber, "Job Receipt Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to generate job receipt number."));
            }
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var jobReceiptList = await _jobReceiptService.GetJobReceipts();
                return Ok(ApiResponse<object>.SuccessResponse(jobReceiptList, "Job Receipts are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch job receipts."));
            }
        }
        [HttpGet("{Job_Receipt_Id}")]
        public async Task<IActionResult> Get(Guid Job_Receipt_Id)
        {
            try
            {
                var jobReceipt = await _jobReceiptService.GetJobReceipt(Job_Receipt_Id);
                return Ok(ApiResponse<object>.SuccessResponse(jobReceipt, "Job Receipt are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch job receipts."));
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JobReceiptModel JobReceipt)
        {
            try
            {
                var result = await _jobReceiptService.SaveJobReceipt(JobReceipt);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Job Receipt are saved successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to save job receipt."));
            }
        }
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] JobReceiptModel JobReceipt)
        {
            try
            {
                var result = await _jobReceiptService.UpdateJobReceipt(JobReceipt);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Job Receipt Updated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to Update job receipt."));
            }
        }
        [HttpDelete("{Job_Receipt_Id}")]
        public async Task<IActionResult> Delete(Guid Job_Receipt_Id)
        {
            try
            {
                var result = await _jobReceiptService.DeleteJobReceipt(Job_Receipt_Id);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Job Receipt deleted successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to delete job receipt."));
            }
        }

        [HttpGet("GetOpneingVoucherBalance")]
        public async Task<IActionResult> GetOpeningVoucerBalance([FromQuery] Guid supplierId)
        {
            try
            {
            var result = await _jobReceiptService.GetOpeningVoucerBalance(supplierId);
                return Ok(ApiResponse<JobReceiptAdvancePaymentDTO>.SuccessResponse(result, "Opeing Voucher Balance fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Unable to fetch opening voucher balance."));
            }
        }
    }
}
