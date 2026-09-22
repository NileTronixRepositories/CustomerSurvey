using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplateLogo;

internal sealed class DeleteTemplateLogoCommandHandler
    : ICommandHandler<DeleteTemplateLogoCommand, DeleteTemplateLogoResponse>
{
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTemplateLogoCommandHandler(
        IWriteReadRepository<Template> templateReadRepository,
        IWriteRepository<Template> templateWriteRepository,
        ICurrentBranchScopeResolver branchScopeResolver,
        IMediaService mediaService,
        IUnitOfWork unitOfWork)
    {
        _templateReadRepository = templateReadRepository;
        _templateWriteRepository = templateWriteRepository;
        _branchScopeResolver = branchScopeResolver;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeleteTemplateLogoResponse>> Handle(
        DeleteTemplateLogoCommand request,
        CancellationToken cancellationToken)
    {
        var scope = await _branchScopeResolver.ResolveAsync(cancellationToken);
        if (scope.IsFailure)
        {
            return Result<DeleteTemplateLogoResponse>.Fail(scope.Errors);
        }

        var template = await _templateReadRepository.GetByPropertyAsync(
            x => x.Id == request.TemplateId && x.BranchId == scope.Value.BranchId,
            cancellationToken);

        if (template is null)
        {
            return Result<DeleteTemplateLogoResponse>.Fail(new Error(
                "Templates.Logo.TemplateNotFound",
                "Template was not found in the current branch.",
                ErrorType.NotFound));
        }

        var oldLogoPath = template.LogoPath;
        template.ClearLogo();
        _templateWriteRepository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldLogoPath))
        {
            _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(oldLogoPath));
        }

        return Result<DeleteTemplateLogoResponse>.Ok(
            new DeleteTemplateLogoResponse(template.Id, template.LogoPath));
    }
}
