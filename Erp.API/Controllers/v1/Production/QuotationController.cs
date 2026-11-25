using Erp.Domain.Model;
using Erp.Domain.Model.Production;
using Erp.Services.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Erp.API.Controllers.v1.Production
{
    [Route("api/v1/production/[controller]")]
    [ApiController]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;
        private const long MaxImageRequestSizeBytes = MaxImageSizeBytes + 64 * 1024;
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationController> _logger;
        private readonly IWebHostEnvironment _environment;

        public QuotationController(
            IQuotationService quotationService,
            ILogger<QuotationController> logger,
            IWebHostEnvironment environment)
        {
            _quotationService = quotationService;
            _logger = logger;
            _environment = environment;
        }

        [HttpGet("GenerateDocumentNumber")]
        public async Task<IActionResult> GenerateDocumentNumber()
        {
            try
            {
                var quotationNumber = await _quotationService.GenerateDocumentNumber();
                return Ok(ApiResponse<object>.SuccessResponse(
                    quotationNumber,
                    "Quotation Number generated successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quotation number.");
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    ex.Message,
                    "Failed to generate quotation number."));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var quotations = await _quotationService.GetQuotations();
                return Ok(ApiResponse<object>.SuccessResponse(quotations, "Quotation list fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotation list.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch quotation list."));
            }
        }

        [HttpGet("{Quotation_Id:guid}")]
        public async Task<IActionResult> Get(Guid Quotation_Id)
        {
            try
            {
                var quotation = await _quotationService.GetQuotation(Quotation_Id);
                return quotation.Id == 0
                    ? NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."))
                    : Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation fetched successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quotation {QuotationId}.", Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to fetch quotation."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QuotationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidateValidity(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var quotation = await _quotationService.Save(model);
                return Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation saved successfully..!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving quotation.");
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to save quotation."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] QuotationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Invalid request data."));
            }

            var validationError = ValidateValidity(model);
            if (validationError != null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, validationError));
            }

            try
            {
                var quotation = await _quotationService.Update(model);
                return Ok(ApiResponse<object>.SuccessResponse(quotation, "Quotation updated successfully..!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(ex.Message, "Quotation not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quotation {QuotationId}.", model.Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to update quotation."));
            }
        }

        [HttpDelete("{Quotation_Id:guid}")]
        public async Task<IActionResult> Delete(Guid Quotation_Id, [FromQuery] string? DeletedBy)
        {
            try
            {
                var quotation = await _quotationService.GetQuotation(Quotation_Id);
                var deleted = await _quotationService.Delete(Quotation_Id, DeletedBy);
                if (deleted)
                {
                    DeleteQuotationImage(quotation.QuotationImagePath);
                }

                return deleted
                    ? Ok(ApiResponse<object>.SuccessResponse(true, "Quotation deleted successfully..!"))
                    : NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
            }
            catch (ConversionConflictException ex)
            {
                return Conflict(ApiResponse<string>.ErrorResponse(null, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting quotation {QuotationId}.", Quotation_Id);
                return StatusCode(500, ApiResponse<string>.ErrorResponse(null, "Failed to delete quotation."));
            }
        }

        [HttpPost("{Quotation_Id:guid}/image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxImageRequestSizeBytes)]
        public async Task<IActionResult> UploadImage(Guid Quotation_Id, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "An image file is required."));
            }

            if (image.Length > MaxImageSizeBytes)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(null, "Image files must be 5 MB or smaller."));
            }

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!IsSupportedImage(image, extension) || !await HasValidImageSignature(image, extension))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    null,
                    "Upload a valid PNG, JPEG, GIF, or WebP image."));
            }

            var quotation = await _quotationService.GetQuotation(Quotation_Id);
            if (quotation.Id == 0)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
            }

            var directory = GetQuotationImageDirectory();
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(directory, fileName);
            var publicPath = $"/uploads/quotations/{fileName}";

            try
            {
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    true))
                {
                    await image.CopyToAsync(stream);
                }

                if (!await _quotationService.UpdateImagePath(Quotation_Id, publicPath))
                {
                    DeleteQuotationImage(publicPath);
                    return NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
                }
            }
            catch
            {
                DeleteQuotationImage(publicPath);
                throw;
            }

            DeleteQuotationImage(quotation.QuotationImagePath);
            return Ok(ApiResponse<object>.SuccessResponse(
                new { quotationImagePath = publicPath },
                "Quotation image uploaded successfully."));
        }

        [HttpDelete("{Quotation_Id:guid}/image")]
        public async Task<IActionResult> DeleteImage(Guid Quotation_Id)
        {
            var quotation = await _quotationService.GetQuotation(Quotation_Id);
            if (quotation.Id == 0)
            {
                return NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
            }

            if (!await _quotationService.UpdateImagePath(Quotation_Id, null))
            {
                return NotFound(ApiResponse<string>.ErrorResponse(null, "Quotation not found."));
            }

            DeleteQuotationImage(quotation.QuotationImagePath);
            return Ok(ApiResponse<string>.SuccessResponse("", "Quotation image deleted successfully."));
        }

        private string GetQuotationImageDirectory()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            return Path.Combine(webRoot, "uploads", "quotations");
        }

        private void DeleteQuotationImage(string? publicPath)
        {
            const string imagePathPrefix = "/uploads/quotations/";
            if (string.IsNullOrWhiteSpace(publicPath) ||
                !publicPath.StartsWith(imagePathPrefix, StringComparison.Ordinal))
            {
                return;
            }

            var fileName = publicPath.Substring(imagePathPrefix.Length);
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            {
                return;
            }

            var filePath = Path.Combine(GetQuotationImageDirectory(), fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private static bool IsSupportedImage(IFormFile image, string extension)
        {
            var expectedContentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => null
            };

            return expectedContentType != null &&
                string.Equals(image.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<bool> HasValidImageSignature(IFormFile image, string extension)
        {
            var header = new byte[12];
            await using var stream = image.OpenReadStream();
            var bytesRead = 0;
            while (bytesRead < header.Length)
            {
                var read = await stream.ReadAsync(header.AsMemory(bytesRead));
                if (read == 0)
                {
                    break;
                }
                bytesRead += read;
            }

            return extension switch
            {
                ".jpg" or ".jpeg" => bytesRead >= 3 &&
                    header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => bytesRead >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
                ".gif" => bytesRead >= 6 &&
                    header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 &&
                    header[3] == 0x38 && (header[4] == 0x37 || header[4] == 0x39) && header[5] == 0x61,
                ".webp" => bytesRead >= 12 &&
                    header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                    header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
                _ => false
            };
        }

        private static string? ValidateValidity(QuotationModel model)
        {
            if (model.Validity_Days < 0 ||
                (model.Quotation_Date.HasValue &&
                 model.Validity_Days > (DateTime.MaxValue.Date - model.Quotation_Date.Value.Date).TotalDays))
            {
                return "Validity_Days must be non-negative and produce a valid date.";
            }
            return null;
        }
    }
}