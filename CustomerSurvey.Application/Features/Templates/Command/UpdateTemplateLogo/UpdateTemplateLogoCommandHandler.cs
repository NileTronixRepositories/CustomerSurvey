using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Templates.Command.UpdateTemplateLogo;

internal sealed class UpdateTemplateLogoCommandHandler
    : ICommandHandler<UpdateTemplateLogoCommand, UpdateTemplateLogoResponse>
{
    private readonly IWriteReadRepository<Template> _templateReadRepository;
    private readonly IWriteRepository<Template> _templateWriteRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTemplateLogoCommandHandler(
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

    public async Task<Result<UpdateTemplateLogoResponse>> Handle(
        UpdateTemplateLogoCommand request,
        CancellationToken cancellationToken)
    {
        var scope = await _branchScopeResolver.ResolveAsync(cancellationToken);
        if (scope.IsFailure)
        {
            return Result<UpdateTemplateLogoResponse>.Fail(scope.Errors);
        }

        var template = await _templateReadRepository.GetByPropertyAsync(
            x => x.Id == request.TemplateId && x.BranchId == scope.Value.BranchId,
            cancellationToken);

        if (template is null)
        {
            return Result<UpdateTemplateLogoResponse>.Fail(new Error(
                "Templates.Logo.TemplateNotFound",
                "Template was not found in the current branch.",
                ErrorType.NotFound));
        }

        var validationError = TemplateLogoMedia.Validate(request.Logo);
        if (validationError is not null)
        {
            return Result<UpdateTemplateLogoResponse>.Fail(validationError);
        }

        var oldLogoPath = template.LogoPath;
        var fileName = await _mediaService.SaveAsync(request.Logo, FileNames.TemplateLogos);
        var newLogoPath = TemplateLogoMedia.ToRelativePath(fileName);

        try
        {
            template.SetLogoPath(newLogoPath);
            _templateWriteRepository.Update(template);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(newLogoPath));
            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldLogoPath))
        {
            _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(oldLogoPath));
        }

        return Result<UpdateTemplateLogoResponse>.Ok(
            new UpdateTemplateLogoResponse(template.Id, newLogoPath));
    }
}
