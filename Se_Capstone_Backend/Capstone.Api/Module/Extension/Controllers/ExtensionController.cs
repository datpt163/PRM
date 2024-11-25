using Capstone.Application.Common.Cloudinaries;
using Capstone.Application.Common.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Api.Module.Extension.Controllers
{
    [Route("api/extensions")]
    [ApiController]
    public class ExtensionController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly PdfReaderService _pdfReaderService;

        public ExtensionController(CloudinaryService cloudinaryService, PdfReaderService pdfReaderService)
        {
            _cloudinaryService = cloudinaryService;
            _pdfReaderService = pdfReaderService;
        }

        [HttpPost("images")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            using var stream = image.OpenReadStream();
            var url = await _cloudinaryService.UploadImageAsync(stream, image.FileName);
            return Created(url, new { Url = url }); 
        }

        [HttpDelete("images/{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            var success = await _cloudinaryService.DeleteImageByUrlAsync(publicId);
            if (success)
            {
                return NoContent(); 
            }
            return NotFound("Image not found or could not be deleted.");
        }

        [HttpPost("pdfs")]
        public IActionResult ExtractPdfContent(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("PDF file is required.");
            }

            using var stream = pdfFile.OpenReadStream();
            var content = _pdfReaderService.ExtractTextFromPdf(stream);

            return Ok(new { Content = content });
        }
    }
}
