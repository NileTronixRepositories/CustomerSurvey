using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Services
{
    public interface IPublicSurveyUrlBuilder
    {
        string BuildAnonymousTemplateUrl(Guid anonymousTemplateId);
    }
}