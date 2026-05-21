using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Reports
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderViewAsync(
            string viewName,
            bool isDraft,
            CancellationToken cancellationToken = default);

        Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model,
            bool isDraft,
            CancellationToken cancellationToken = default);
    }
}