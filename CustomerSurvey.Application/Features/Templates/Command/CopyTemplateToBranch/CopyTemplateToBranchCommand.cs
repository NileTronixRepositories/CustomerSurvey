using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.Templates.Command.CopyTemplateToBranch;

public sealed record CopyTemplateToBranchCommand
    : ICommand<CopyTemplateToBranchResponse>
{
    public Guid TemplateId { get; init; }

    public Guid BranchId { get; init; }
}
