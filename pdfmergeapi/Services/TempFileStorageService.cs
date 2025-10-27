 using Microsoft.AspNetCore.Http;

    namespace PdfMergeApi.Services
    {
        public class TempFileStorageService : IFileStorageService
        {
            private readonly string _tempDir;

            public TempFileStorageService()
            {
                _tempDir = Path.Combine(Path.GetTempPath(), "PdfMergeApi");
                if (!Directory.Exists(_tempDir)) Directory.CreateDirectory(_tempDir);
            }

            public async Task<string> SaveTempFileAsync(IFormFile file)
            {
                var safeName = Path.GetFileName(file.FileName);
                var dest = Path.Combine(_tempDir, $"{Guid.NewGuid()}_{safeName}");
                await using var stream = new FileStream(dest, FileMode.Create);
                await file.CopyToAsync(stream);
                return dest;
            }

            public Task DeleteTempFileAsync(string path)
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch { }
                return Task.CompletedTask;
            }
        }
    }
