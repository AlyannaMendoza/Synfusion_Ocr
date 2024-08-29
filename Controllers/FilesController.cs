using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SYNCFUSION_TRIAL.Data;
using SYNCFUSION_TRIAL.Models;
using SYNCFUSION_TRIAL.Services;
using System.Net.Mime;

namespace SYNCFUSION_TRIAL.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOcrService _ocrService;

        public FilesController(ApplicationDbContext context, IOcrService ocrService)
        {
            _context = context;
            _ocrService = ocrService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var files = await _context.fileMetadata.ToListAsync();
            return View(files);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            var allowedContentTypes = new List<string> { "application/pdf", "image/jpeg", "image/jpg", "image/png" };

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                throw new InvalidOperationException("Unsupported file type.");
            }

            var uploadDate = DateTime.Now;
            var monthFolder = $"{uploadDate.ToString("MMMM")} {uploadDate.Year.ToString()}";
            var dayFolder = $"{uploadDate.ToString("MMMM")} {uploadDate.Day} {uploadDate.Year.ToString()}";

            var folderPath = Path.Combine(
               Directory.GetCurrentDirectory(),
               "wwwroot",
               "Documents Uploaded",
               uploadDate.Year.ToString(),
               monthFolder,
               dayFolder
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var uniqueFileName = $"{timestamp}_{file.FileName}_{file.Name}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string ocrResult = string.Empty;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                if (file.ContentType == "application/pdf")
                {
                    ocrResult = await _ocrService.PerformSyncfusionOcrAsync(memoryStream);
                }
                else
                {
                    ocrResult = await _ocrService.PerformTesseractOcrAsync(memoryStream);

                    // Generate searchable PDF
                   //var pdfBytes = await _ocrService.GenerateSearchablePdfAsync(memoryStream, ocrResult, "Document Title", uniqueFileName);

                   // using (var pdfStream = new FileStream(filePath, FileMode.Create))
                   // {
                   //     await pdfStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                   // }
                }

                // Read the PDF data into a byte array
                byte[] fileData;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var fsMemoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(fsMemoryStream);
                        fileData = fsMemoryStream.ToArray();
                    }
                }

                var uploadedFileMetadata = new FileMetadata
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = new FileInfo(filePath).Length,
                    FilePath = filePath,
                    UploadDate = uploadDate,
                };

                var uploadedFileData = new FileData
                {
                    Data = fileData, // PDF data
                    FileMetadata = uploadedFileMetadata
                };

                var existingFileData = _context.fileData.FirstOrDefault(fd => fd.FileMetadata.FileName == file.FileName);
                if (existingFileData != null)
                {
                    _context.fileData.Remove(existingFileData);
                }

                _context.fileMetadata.Add(uploadedFileMetadata);
                _context.fileData.Add(uploadedFileData);
                await _context.SaveChangesAsync();

                var ocrResultRecord = new OcrResult
                {
                    FileMetadataId = uploadedFileMetadata.Id,
                    ExtractedText = ocrResult
                };
                
                _context.ocrResults.Add(ocrResultRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var file = await _context.fileMetadata
                .Include(f => f.OcrResult) // Ensure the related OcrResult is loaded
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            return View(file);
        }

        [HttpGet]
        public async Task<IActionResult> GetFile(int id)
        {
            var fileData = await _context.fileData
                .Include(fd => fd.FileMetadata)
                .FirstOrDefaultAsync(fd => fd.FileMetadataId == id);

            if (fileData == null)
            {
                return NotFound();
            }

            var contentType = fileData.FileMetadata.ContentType;
            var fileName = fileData.FileMetadata.FileName;

            Response.Headers.Append("Content-Disposition", $"inline; filename={fileName}");

            return File(fileData.Data, contentType);
        }
    }
}
