
    using PdfMergeApi.Models;

    namespace PdfMergeApi.Services
    {
        public interface IDocumentService
        {
            Task<byte[]> MergeSequenceAsync(List<MergeItem> sequence, Dictionary<string, string> uploadedPaths);
        }
    }
