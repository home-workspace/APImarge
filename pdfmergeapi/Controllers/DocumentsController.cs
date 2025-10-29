
using Microsoft.AspNetCore.Mvc;
using PdfMergeApi.Models;
    using PdfMergeApi.Services;
using System.Text.Json;


namespace PdfMergeApi.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class DocumentsController : ControllerBase
        {
            private readonly IDocumentService _documentService;
            private readonly IFileStorageService _fileStorage;
          

            public DocumentsController(IDocumentService documentService, IFileStorageService fileStorage)
            {
                _documentService = documentService;
                _fileStorage = fileStorage;
            }

            /// <summary>
            /// Upload files (pdfs and images) and pass JSON sequence to merge and insert images.
            /// Multipart/form-data: files (field "files") + a JSON field "sequence" containing a MergeRequest.
            /// </summary>
            [HttpPost("merge")]
            [RequestSizeLimit(200_000_000)]
            [Consumes("multipart/form-data")]
        public async Task<IActionResult> Merge([FromForm] MergeRequest merge, [FromForm] List<IFormFile> Files)
        {
            // 1. Validar 
            var validationResult = merge.Validator(merge, Files);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    Message = validationResult.Message,
                    Errors = validationResult.Errors
                });
            }

            // 2. Guardar archivos temporalmente (ya validados)
            var savedFiles = new Dictionary<string, string>();
            try
            {
                foreach (var file in Files)
                {
                    var saved = await _fileStorage.SaveTempFileAsync(file);
                    savedFiles[file.FileName] = saved;
                }

                // 3. Procesar merge
                var outputBytes = await _documentService.MergeSequenceAsync(validationResult.Items, savedFiles);

                // 4. Validar resultado del merge
                if (outputBytes == null || outputBytes.Length == 0)
                {
                    throw new Exception("Merge process did not produce any output");
                }

                // 5. Limpiar y retornar
                foreach (var path in savedFiles.Values)
                {
                    await _fileStorage.DeleteTempFileAsync(path);
                }

                return Ok(new
                {
                    status = "success",
                    message = "Files merged successfully",
                    outputFile = File(outputBytes, "application/pdf", merge.OutputFileName ?? "merged.pdf")
                });

            }
            catch (Exception ex)
            {
                // Limpiar archivos temporales en caso de error
                foreach (var path in savedFiles.Values)
                {
                    try { await _fileStorage.DeleteTempFileAsync(path); } catch { }
                }
                return StatusCode(500, $"Error during merge process: {ex.Message}");
            }
        }
    }
    }
