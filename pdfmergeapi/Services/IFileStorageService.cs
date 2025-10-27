
    namespace PdfMergeApi.Services
    {
        public interface IFileStorageService
        {
            Task<string> SaveTempFileAsync(Microsoft.AspNetCore.Http.IFormFile file);
            Task DeleteTempFileAsync(string path);
        }
    }
