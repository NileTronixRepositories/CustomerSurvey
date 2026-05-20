using CustomerSurvey.Application.Abstraction.Services;
using CustomerSurvey.infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CustomerSurvey.infrastructure.Services
{
    internal sealed class PublicSurveyUrlBuilder : IPublicSurveyUrlBuilder
    {
        private readonly PublicSurveyOptions _options;

        public PublicSurveyUrlBuilder(IOptions<PublicSurveyOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public string BuildAnonymousTemplateUrl(Guid anonymousTemplateId)
        {
            if (string.IsNullOrWhiteSpace(_options.AnonymousTemplateBaseUrl))
            {
                throw new InvalidOperationException(
                    "PublicSurvey:AnonymousTemplateBaseUrl is not configured.");
            }

            var baseUrl = _options.AnonymousTemplateBaseUrl.Trim().TrimEnd('/');

            return $"{baseUrl}/{anonymousTemplateId}";
        }
    }
}