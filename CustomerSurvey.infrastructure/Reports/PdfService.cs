using System.Globalization;
using CustomerSurvey.Application.Abstraction.Reports;
using Microsoft.Extensions.Logging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace CustomerSurvey.infrastructure.Reports
{
    internal sealed class PdfService : IPdfService
    {
        private static readonly SemaphoreSlim BrowserDownloadLock = new(1, 1);
        private static bool _browserDownloaded;

        private readonly IRazorViewRenderer _razorRenderer;
        private readonly ILogger<PdfService> _logger;

        public PdfService(
            IRazorViewRenderer razorRenderer,
            ILogger<PdfService> logger)
        {
            _razorRenderer = razorRenderer ?? throw new ArgumentNullException(nameof(razorRenderer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> GeneratePdfAsync<TModel>(
            string viewName,
            TModel model,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default)
        {
            var htmlContent = await _razorRenderer.RenderViewToStringAsync(
                viewName,
                model,
                options.IsDraft,
                cancellationToken);

            return await GeneratePdfFromHtmlAsync(
                htmlContent,
                options,
                cancellationToken);
        }

        public async Task<byte[]> GeneratePdfAsync(
            string viewName,
            PdfRenderOptions options,
            CancellationToken cancellationToken = default)
        {
            var htmlContent = await _razorRenderer.RenderViewAsync(
                viewName,
                options.IsDraft,
                cancellationToken);

            return await GeneratePdfFromHtmlAsync(
                htmlContent,
                options,
                cancellationToken);
        }

        private async Task<byte[]> GeneratePdfFromHtmlAsync(
            string htmlContent,
            PdfRenderOptions options,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                throw new ArgumentException("HTML content cannot be empty.", nameof(htmlContent));
            }

            await EnsureBrowserDownloadedAsync(cancellationToken);

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Timeout = 1_000_000,
                Args = new[]
                {
                    "--disable-web-security",
                    "--allow-file-access",
                    "--allow-file-access-from-files",
                    "--no-sandbox",
                    "--disable-setuid-sandbox"
                }
            };

            try
            {
                await using var browser = await Puppeteer.LaunchAsync(launchOptions);
                await using var page = await browser.NewPageAsync();

                await page.SetContentAsync(
                    htmlContent,
                    new NavigationOptions
                    {
                        WaitUntil = new[]
                        {
                            WaitUntilNavigation.Load,
                            WaitUntilNavigation.Networkidle0
                        },
                        Timeout = 1_000_000
                    });

                await page.EvaluateExpressionHandleAsync("document.fonts.ready");

                return await page.PdfDataAsync(new PdfOptions
                {
                    PrintBackground = options.PrintBackground,
                    DisplayHeaderFooter = options.DisplayHeaderFooter,
                    Landscape = options.IsLandscape,
                    Format = PaperFormat.A4,
                    MarginOptions = new MarginOptions
                    {
                        Top = ToCssCm(options.MarginTopCm),
                        Right = ToCssCm(options.MarginRightCm),
                        Bottom = ToCssCm(options.MarginBottomCm),
                        Left = ToCssCm(options.MarginLeftCm)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to generate PDF using PuppeteerSharp.");

                throw;
            }
        }

        private static string ToCssCm(decimal value)
        {
            return $"{value.ToString("0.###", CultureInfo.InvariantCulture)}cm";
        }

        private static async Task EnsureBrowserDownloadedAsync(
            CancellationToken cancellationToken)
        {
            if (_browserDownloaded)
            {
                return;
            }

            await BrowserDownloadLock.WaitAsync(cancellationToken);

            try
            {
                if (_browserDownloaded)
                {
                    return;
                }

                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _browserDownloaded = true;
            }
            finally
            {
                BrowserDownloadLock.Release();
            }
        }

        public byte[] MergePdfByteArrays(byte[] pdf1, byte[] pdf2)
        {
            return MergePdfByteArrays(new[] { pdf1, pdf2 });
        }

        public byte[] MergePdfByteArrays(IReadOnlyCollection<byte[]> pdfs)
        {
            if (pdfs is null || pdfs.Count == 0)
            {
                return Array.Empty<byte>();
            }

            using var outputDocument = new PdfDocument();

            foreach (var pdf in pdfs)
            {
                if (pdf is null || pdf.Length == 0)
                {
                    continue;
                }

                using var inputStream = new MemoryStream(pdf);
                using var inputDocument = PdfReader.Open(
                    inputStream,
                    PdfDocumentOpenMode.Import);

                for (var i = 0; i < inputDocument.PageCount; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }
            }

            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream, false);

            return outputStream.ToArray();
        }

        public bool IsPdfEmpty(byte[] pdfBytes)
        {
            if (pdfBytes is null || pdfBytes.Length == 0)
            {
                return true;
            }

            using var ms = new MemoryStream(pdfBytes);
            using var pdfDocument = PdfReader.Open(
                ms,
                PdfDocumentOpenMode.Import);

            return pdfDocument.PageCount == 0;
        }
    }
}