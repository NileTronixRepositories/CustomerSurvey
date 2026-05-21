using CustomerSurvey.Application.Abstraction.Reports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RazorLight;
using System.Dynamic;

namespace CustomerSurvey.infrastructure.Reports
{
    internal sealed class RazorViewRenderer : IRazorViewRenderer
    {
        private readonly RazorLightEngine _razorEngine;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<RazorViewRenderer> _logger;
        private readonly string _templatesPath;

        public RazorViewRenderer(
            IWebHostEnvironment env,
            ILogger<RazorViewRenderer> logger)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _templatesPath = Path.Combine(
                _env.ContentRootPath,
                "Views",
                "Templates");

            _logger.LogInformation("RazorLight templates path: {TemplatesPath}", _templatesPath);
            _logger.LogInformation(
                "BranchTemplatesPdfReport exists: {Exists}",
                File.Exists(Path.Combine(_templatesPath, "BranchTemplatesPdfReport.cshtml")));

            if (!Directory.Exists(_templatesPath))
            {
                throw new DirectoryNotFoundException(
                    $"Razor templates folder was not found: {_templatesPath}");
            }

            _razorEngine = new RazorLightEngineBuilder()
                .UseFileSystemProject(_templatesPath)
                .UseMemoryCachingProvider()
                .Build();
        }

        public async Task<string> RenderViewAsync(
            string viewName,
            bool isDraft,
            CancellationToken cancellationToken = default)
        {
            var model = new
            {
                Title = string.Empty
            };

            return await RenderViewToStringAsync(
                viewName,
                model,
                isDraft,
                cancellationToken);
        }

        public async Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model,
            bool isDraft,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var templateName = NormalizeTemplateName(viewName);
                var templateFullPath = Path.Combine(_templatesPath, templateName);

                if (!File.Exists(templateFullPath))
                {
                    throw new FileNotFoundException(
                        $"Razor template was not found. TemplateName: {templateName}, FullPath: {templateFullPath}",
                        templateFullPath);
                }

                dynamic viewBag = new ExpandoObject();
                viewBag.IsDraft = isDraft;
                viewBag.LogoBase64 = await TryGetLogoBase64Async(cancellationToken);

                return await _razorEngine.CompileRenderAsync(
                    templateName,
                    model,
                    viewBag);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to render Razor template {ViewName}. ContentRootPath: {ContentRootPath}, TemplatesPath: {TemplatesPath}",
                    viewName,
                    _env.ContentRootPath,
                    _templatesPath);

                throw new InvalidOperationException(
                    $"Failed to render template '{viewName}'.",
                    ex);
            }
        }

        private static string NormalizeTemplateName(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentException("View name cannot be empty.", nameof(viewName));
            }

            var normalized = viewName
                .Trim()
                .Replace("\\", "/")
                .TrimStart('/');

            return normalized.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : $"{normalized}.cshtml";
        }

        private async Task<string> TryGetLogoBase64Async(
            CancellationToken cancellationToken)
        {
            var webRootPath = _env.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                return string.Empty;
            }

            var logoPath = Path.Combine(
                webRootPath,
                "assets",
                "logo.png");

            if (!File.Exists(logoPath))
            {
                return string.Empty;
            }

            var bytes = await File.ReadAllBytesAsync(
                logoPath,
                cancellationToken);

            return Convert.ToBase64String(bytes);
        }
    }
}