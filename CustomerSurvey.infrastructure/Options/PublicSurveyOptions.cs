using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Options
{
    public sealed class PublicSurveyOptions
    {
        public const string SectionName = "PublicSurvey";

        public string AnonymousTemplateBaseUrl { get; init; } = string.Empty;
    }
}