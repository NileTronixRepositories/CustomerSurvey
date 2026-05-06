using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Options
{
    public sealed class CorsSettings
    {
        public bool AllowAnyOrigin { get; set; }
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}