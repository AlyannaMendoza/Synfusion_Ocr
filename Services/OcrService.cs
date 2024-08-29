using Syncfusion.OCRProcessor;
using Syncfusion.Pdf.Exporting;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;
using SYNCFUSION_TRIAL.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Tesseract;

namespace SYNCFUSION_TRIAL.Services
{
    public interface IOcrService
    {
        Task<string> PerformSyncfusionOcrAsync(Stream fileStream);
        Task<string> PerformTesseractOcrAsync(Stream fileStream);
        //Task<byte[]> GenerateSearchablePdfAsync(Stream imageStream, string extractedText, string documentTitle, string outputFileName);
    }

    public class OcrService : IOcrService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _tessdataPath;

        public OcrService(ApplicationDbContext context)
        { 
            _context = context; 
            _tessdataPath = Path.Combine(Environment.CurrentDirectory, "tessdata");
        }

        public async Task<string> PerformSyncfusionOcrAsync(Stream fileStream)
        {
            return await Task.Run(() => 
            {
                StringBuilder extractedText = new StringBuilder();

                try
                {
                    using (OCRProcessor processor = new OCRProcessor())
                    {
                        using (PdfLoadedDocument loadedDoc = new PdfLoadedDocument(fileStream))
                        {
                            processor.Settings.Language = Languages.English;

                            string allText = processor.PerformOCR(loadedDoc);

                            // Append the text from the entire document to the result
                            extractedText.Append(allText);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the OCR process
                    throw new InvalidOperationException("An error occurred during the Syncfusion OCR process.", ex);
                }

                return extractedText.ToString();
            });
        }

        public async Task<string> PerformTesseractOcrAsync(Stream fileStream)
        {
            return await Task.Run(() =>
            {
                fileStream.Position = 0;
                string extractedText = string.Empty;

                using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(((MemoryStream)fileStream).ToArray()))
                    {
                        using (var page = engine.Process(img))
                        {
                            extractedText = page.GetText();
                        }
                    }
                }

                return extractedText;
            });
        }

        //public async Task<byte[]> GenerateSearchablePdfAsync(Stream imageStream, string extractedText, string documentTitle, string outputFileName)
        //{
        //    return await Task.Run(() =>
        //    {
                //var tempPdfLocation = Path.Combine(_tessdataPath, "pdf");
                //var tempFile = Path.Combine(tempPdfLocation, pdfFileName);

                //using (var renderer = PdfResultRenderer.CreatePdfRenderer(tempFile, _tessdataPath, false))
                //{
                //    using (renderer.BeginDocument(pdfTitle))
                //    {
                //        using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.TesseractAndLstm))
                //        {
                //            using (var img = Pix.LoadFromMemory(((MemoryStream)imageStream).ToArray()))
                //            {
                //                using (var page = engine.Process(img, pdfTitle))
                //                {
                //                    renderer.AddPage(page);
                //                }
                //            }
                //        }
                //    }
                //}

                //byte[] pdfBytes;
                //using (var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                //{
                //    using (var memoryStream = new MemoryStream())
                //    {
                //        fileStream.CopyTo(memoryStream);
                //        pdfBytes = memoryStream.ToArray();
                //    }
                //}
                //File.Delete(tempFile);

                //return pdfBytes;

                //        string tempPdfPath = Path.Combine(Path.GetTempPath(), outputFileName);

                //        // Return the PDF file as a byte array
                //        byte[] pdfBytes;

                //        try
                //        {
                //            imageStream.Position = 0;
                //            using (var renderer = Tesseract.PdfResultRenderer.CreatePdfRenderer(tempPdfPath, _tessdataPath, false))
                //            {
                //                using (var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.TesseractAndLstm))
                //                {
                //                    using (var img = Pix.LoadFromMemory(((MemoryStream)imageStream).ToArray()))
                //                    {
                //                        using (var page = engine.Process(img, documentTitle))
                //                        {
                //                            renderer.AddPage(page); // Add page to the renderer
                //                        }
                //                    }
                //                }
                //            }

                //            using (var fileStream = new FileStream(tempPdfPath, FileMode.Open, FileAccess.Read))
                //            {
                //                using (var memoryStream = new MemoryStream())
                //                {
                //                    fileStream.CopyToAsync(memoryStream);
                //                    pdfBytes = memoryStream.ToArray();
                //                }
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            // Log or handle the exception as needed
                //            throw new InvalidOperationException("An error occurred while generating the searchable PDF.", ex);
                //        }
                //        finally
                //        {
                //            // Clean up temporary file
                //            if (File.Exists(tempPdfPath))
                //            {
                //                File.Delete(tempPdfPath);
                //            }
                //        }

                //        return pdfBytes;
                //    });
            //}
    }
}
