
    using PdfMergeApi.Models;
    using PdfSharpCore.Pdf;
    using PdfSharpCore.Pdf.IO;
    using PdfSharpCore.Drawing;
using System.Text.Json;

namespace PdfMergeApi.Services
    {
        public class PdfDocumentService : IDocumentService
        {
        public async Task<byte[]> MergeSequenceAsync(List<MergeItem> sequence, Dictionary<string, string> uploadedPaths)
        {
            // Usamos una lista temporal de páginas
            var allPages = new List<PdfPage>();


           


            foreach (MergeItem item in sequence)
            {
                if (!uploadedPaths.TryGetValue(item.FileName, out var path))
                    throw new FileNotFoundException("Referenced file not found", item.FileName);

                var ext = Path.GetExtension(path).ToLowerInvariant();
                var pagesToInsert = new List<PdfPage>();

                if (item.ContentType.Equals("pdf", StringComparison.OrdinalIgnoreCase) || ext == ".pdf")
                {
                    using var input = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                    for (int i = 0; i < input.PageCount; i++)
                        pagesToInsert.Add(input.Pages[i]);
                }
                else if (item.ContentType.Equals("image", StringComparison.OrdinalIgnoreCase) ||
                         new[] { ".jpg", ".jpeg", ".png", ".bmp" }.Contains(ext))
                {
                    using var imgStream = File.OpenRead(path);
                    var img = XImage.FromStream(() => imgStream);

                    var page = new PdfPage
                    {
                        Width = XUnit.FromPoint(img.PixelWidth * 72.0 / img.HorizontalResolution),
                        Height = XUnit.FromPoint(img.PixelHeight * 72.0 / img.VerticalResolution)
                    };

                    using var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(img, 0, 0, page.Width.Point, page.Height.Point);

                    pagesToInsert.Add(page);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported file type for item: {item.FileName}");
                }

                // Si tiene posición de inserción, la aplicamos
                if (item.InsertAfterPage.HasValue && item.InsertAfterPage.Value < allPages.Count)
                    allPages.InsertRange(item.InsertAfterPage.Value, pagesToInsert);
                else
                    allPages.AddRange(pagesToInsert);
            }

            // Crear documento final
            using var outputDoc = new PdfDocument();
            foreach (var p in allPages)
                outputDoc.AddPage(p);

            await using var ms = new MemoryStream();
            outputDoc.Save(ms, false);
            ms.Position = 0;
            return ms.ToArray();
        }

    }
}
