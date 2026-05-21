using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Reports
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync<TModel>(
            string viewName,
            TModel model,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default);

        Task<byte[]> GeneratePdfAsync(
            string viewName,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default);

        byte[] MergePdfByteArrays(byte[] pdf1, byte[] pdf2);

        byte[] MergePdfByteArrays(IReadOnlyCollection<byte[]> pdfs);

        bool IsPdfEmpty(byte[] pdfBytes);
    }

    public sealed record PdfRenderOptions
    {
        public bool IsDraft { get; init; }

        public bool PrintBackground { get; init; } = true;

        public bool DisplayHeaderFooter { get; init; }

        public bool IsLandscape { get; init; }

        public string Format { get; init; } = "A4";

        public decimal MarginTopCm { get; init; } = 0.7m;

        public decimal MarginRightCm { get; init; } = 0.7m;

        public decimal MarginBottomCm { get; init; } = 0.7m;

        public decimal MarginLeftCm { get; init; } = 0.7m;
    }
}