namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class CopyTemplateToBranchRequest
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }
    }
}
