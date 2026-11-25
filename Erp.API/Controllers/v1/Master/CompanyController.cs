using Erp.Domain.DTO;
using Erp.Domain.Model;
using Erp.Services.Interfaces.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Erp.API.Controllers.v1.Master
{
    [Route("api/v1/masters/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private const long MaxLogoSizeBytes = 5 * 1024 * 1024;
        private const long MaxLogoRequestSizeBytes = MaxLogoSizeBytes + 64 * 1024;
        private readonly ICompanyService _companyService;
        private readonly IWebHostEnvironment _environment;
        public CompanyController(ICompanyService companyService, IWebHostEnvironment environment)
        {
            _companyService = companyService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try {
                var companyList = await _companyService.GetCompanies();
                return Ok(ApiResponse<object>.SuccessResponse(companyList, "Companies are fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse( ex.Message, "Failed to fetch companies."));
            }
        
        }

        [HttpGet("{Company_Id}")]
        public async Task<IActionResult> Get(Guid Company_Id)
        {
            try
            {
                var company = await _companyService.GetCompany(Company_Id);
                return Ok(ApiResponse<object>.SuccessResponse(company, "Company is fetched successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to fetch company."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyDTO Company)
        {
            try
            {
                var company = await _companyService.Save(Company);
                return Ok(ApiResponse<object>.SuccessResponse(company, "Company is saved successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to save company."));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] CompanyDTO Company)
        {
            try
            {
                var company = await _companyService.Update(Company);
                return Ok(ApiResponse<object>.SuccessResponse(company, "Company is updated successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to update company."));
            }
        }

        [HttpPost("{Company_Id:guid}/logo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxLogoRequestSizeBytes)]
        public async Task<IActionResult> UploadLogo(Guid Company_Id, IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("A logo file is required.", "Failed to upload company logo."));
            }

            if (logo.Length > MaxLogoSizeBytes)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Logo files must be 5 MB or smaller.", "Failed to upload company logo."));
            }

            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!IsSupportedLogo(logo, extension) || !await HasValidImageSignature(logo, extension))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Upload a valid PNG, JPEG, GIF, or WebP image.", "Failed to upload company logo."));
            }

            var company = await _companyService.GetCompany(Company_Id);
            if (company.Id == 0)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Company was not found.", "Failed to upload company logo."));
            }

            var directory = GetLogoDirectory();
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(directory, fileName);
            var publicPath = $"/uploads/company-logos/{fileName}";

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
                {
                    await logo.CopyToAsync(stream);
                }

                if (!await _companyService.UpdateLogoPath(Company_Id, publicPath))
                {
                    DeleteLogoFile(publicPath);
                    return NotFound(ApiResponse<string>.ErrorResponse("Company was not found.", "Failed to upload company logo."));
                }
            }
            catch
            {
                DeleteLogoFile(publicPath);
                throw;
            }

            DeleteLogoFile(company.CompanyLogoPath);
            return Ok(ApiResponse<object>.SuccessResponse(
                new { companyLogoPath = publicPath },
                "Company logo is uploaded successfully..!"));
        }

        [HttpDelete("{Company_Id:guid}/logo")]
        public async Task<IActionResult> DeleteLogo(Guid Company_Id)
        {
            var company = await _companyService.GetCompany(Company_Id);
            if (company.Id == 0)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Company was not found.", "Failed to delete company logo."));
            }

            if (!await _companyService.UpdateLogoPath(Company_Id, null))
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Company was not found.", "Failed to delete company logo."));
            }

            DeleteLogoFile(company.CompanyLogoPath);
            return Ok(ApiResponse<string>.SuccessResponse("", "Company logo is deleted successfully..!"));
        }

        [HttpDelete("{Company_Id}")]
        public async Task<IActionResult> Delete(Guid Company_Id)
        {
            try
            {
                var company = await _companyService.GetCompany(Company_Id);
                var deleted = await _companyService.Delete(Company_Id);
                if (deleted)
                {
                    DeleteLogoFile(company.CompanyLogoPath);
                }
                return Ok(ApiResponse<string>.SuccessResponse("", "Company is deleted successfully..!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, "Failed to delete company."));
            }
        }

        private string GetLogoDirectory()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            return Path.Combine(webRoot, "uploads", "company-logos");
        }

        private void DeleteLogoFile(string? publicPath)
        {
            const string logoPathPrefix = "/uploads/company-logos/";
            if (string.IsNullOrWhiteSpace(publicPath) ||
                !publicPath.StartsWith(logoPathPrefix, StringComparison.Ordinal))
            {
                return;
            }

            var fileName = publicPath.Substring(logoPathPrefix.Length);
            if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
            {
                return;
            }

            var filePath = Path.Combine(GetLogoDirectory(), fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        private static bool IsSupportedLogo(IFormFile logo, string extension)
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
                string.Equals(logo.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<bool> HasValidImageSignature(IFormFile logo, string extension)
        {
            var header = new byte[12];
            await using var stream = logo.OpenReadStream();
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
    }
}
