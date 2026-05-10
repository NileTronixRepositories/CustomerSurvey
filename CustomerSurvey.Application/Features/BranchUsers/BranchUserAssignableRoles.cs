using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers
{
    internal static class BranchUserAssignableRoles
    {
        public const string TemplateEditor = "Template Editor";
        public const string QuestionEditor = "Question Editor";
        public const string ReportViewer = "Report Viewer";

        public static readonly IReadOnlyCollection<string> Names =
        [
            TemplateEditor,
            QuestionEditor,
            ReportViewer
        ];

        public static bool IsAllowed(string roleName)
        {
            return Names.Contains(
                roleName,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}