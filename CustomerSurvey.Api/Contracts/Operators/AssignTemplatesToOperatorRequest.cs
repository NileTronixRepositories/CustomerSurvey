namespace CustomerSurvey.Api.Contracts.Operators
{
    public sealed class AssignTemplatesToOperatorRequest
    {
        public IReadOnlyCollection<Guid> TemplateIds { get; init; }
            = Array.Empty<Guid>();
    }
}