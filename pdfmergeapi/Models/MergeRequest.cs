
    using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace PdfMergeApi.Models
{
    public class MergeRequest
    {

        public string? Sequence { get; set; } 
        public string? OutputFileName { get; set; }

        public ValidationResult Validator(MergeRequest merge, List<IFormFile> files)
        {
            var result = new ValidationResult();
            result.IsValid = false;
            result.Message = "";
            result.Errors = new List<string>();


            // Deserializar
            List<MergeItem>? items;
            try
            {
                items = JsonSerializer.Deserialize<List<MergeItem>>(merge.Sequence, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                result.Message = "Invalid JSON format in Sequence";
                return result;
            }

            // Validar deserialización
            if (items == null || !items.Any())
            {
                result.Message = "Could not deserialize items from sequence";
                return result;
            }

            // ========== VALIDACIONES DE ARCHIVOS ==========

            // Validación 1: Verificar que Files no sea null o vacío
            if (files == null || !files.Any())
            {
                result.Errors.Add("No files were uploaded.");
                result.Message = "File validation failed";
                return result;
            }

            // Validación 2: Verificar que hay al menos un archivo PDF
            if (!files.Any(f => f.ContentType?.ToLower().Contains("pdf") == true ||
                               f.FileName?.ToLower().EndsWith(".pdf") == true))
            {
                result.Errors.Add("At least one file must be a PDF.");
            }

            // Validación 3: Verificar tamaño total de archivos
            var maxTotalSize = 200 * 1024 * 1024; // 200 MB
            var totalSize = files.Sum(f => f.Length);
            if (totalSize > maxTotalSize)
            {
                result.Errors.Add($"Total file size ({totalSize / 1024 / 1024} MB) exceeds the maximum allowed ({maxTotalSize / 1024 / 1024} MB).");
            }

            // Validación 4: Verificar cantidad máxima de archivos
            var maxFileCount = 20;
            if (files.Count > maxFileCount)
            {
                result.Errors.Add($"Maximum number of files exceeded. Maximum: {maxFileCount}, Uploaded: {files.Count}.");
            }

            // Validación 5: Verificar nombres de archivo únicos
            var duplicateFiles = files.GroupBy(f => f.FileName)
                                     .Where(g => g.Count() > 1)
                                     .Select(g => g.Key)
                                     .ToList();
            if (duplicateFiles.Any())
            {
                result.Errors.Add($"Duplicate file names: {string.Join(", ", duplicateFiles)}");
            }

            // Validación 6: Verificar que los archivos coincidan con los items del JSON
            var itemFileNames = items.Select(i => i.FileName).ToList();
            var uploadedFileNames = files.Select(f => f.FileName).ToList();

            var missingInItems = uploadedFileNames.Except(itemFileNames).ToList();
            var missingInFiles = itemFileNames.Except(uploadedFileNames).ToList();

            if (missingInItems.Any())
            {
                result.Errors.Add($"The following files were uploaded but are not in the sequence: {string.Join(", ", missingInItems)}");
            }

            if (missingInFiles.Any())
            {
                result.Errors.Add($"The following files are in the sequence but were not uploaded: {string.Join(", ", missingInFiles)}");
            }

            // Validación 7: Verificar tipos de archivo individuales
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp" };
            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file.FileName)?.ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    result.Errors.Add($"File type not allowed: {file.FileName}. Allowed formats: PDF, JPG, PNG, GIF, BMP, TIFF, WEBP");
                }

                // Validación 8: Verificar archivos vacíos
                if (file.Length == 0)
                {
                    result.Errors.Add($"The file is empty: {file.FileName}");
                }

                // Validación 9: Verificar tamaño individual de archivo
                var maxIndividualSize = 50 * 1024 * 1024; // 50 MB por archivo
                if (file.Length > maxIndividualSize)
                {
                    result.Errors.Add($"File {file.FileName} ({file.Length / 1024 / 1024} MB) exceeds the maximum allowed size per file ({maxIndividualSize / 1024 / 1024} MB).");
                }
            }

            // ========== VALIDACIONES DE ITEMS ==========

            // Validaciones de cada item
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];

                if (string.IsNullOrEmpty(item.FileName))
                    result.Errors.Add($"Item {i}: FileName is required");

                if (string.IsNullOrEmpty(item.ContentType))
                    item.ContentType = "application/pdf"; // Default value
                else if (item.ContentType == "pdf")
                    item.ContentType = "application/pdf"; // Correct format

                // Validación adicional: Verificar que InsertAfterPage no sea negativo
                if (item.InsertAfterPage.HasValue && item.InsertAfterPage < 0)
                {
                    result.Errors.Add($"Item {i}: InsertAfterPage cannot be negative");
                }
            }

            // Determinar si la validación fue exitosa
            if (result.Errors.Any())
            {
                result.IsValid = false;
                result.Message = "Validation failed";
            }
            else
            {
                result.IsValid = true;
                result.Message = "Validation successful";
                result.Items = items; // Guardar los items deserializados
            }

            return result;
        }

    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public List<MergeItem> Items { get; set; } = new List<MergeItem>(); 
    }


    public class MergeItem
    {
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "pdf";
        public int? InsertAfterPage { get; set; }
    }

    



}
