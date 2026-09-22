using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplateLogo;

internal sealed class UpdateAnonymousTemplateLogoCommandHandler
    : ICommandHandler<UpdateAnonymousTemplateLogoCommand, UpdateAnonymousTemplateLogoResponse>
{
    private readonly IWriteReadRepository<AnonymousTemplate> _templateReadRepository;
    private readonly IWriteRepository<AnonymousTemplate> _templateWriteRepository;
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAnonymousTemplateLogoCommandHandler(
        IWriteReadRepository<AnonymousTemplate> templateReadRepository,
        IWriteRepository<AnonymousTemplate> templateWriteRepository,
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        ICurrentBranchScopeResolver branchScopeResolver,
        ICurrentUser currentUser,
        IMediaService mediaService,
        IUnitOfWork unitOfWork)
    {
        _templateReadRepository = templateReadRepository;
        _templateWriteRepository = templateWriteRepository;
        _superAdminReadRepository = superAdminReadRepository;
        _branchScopeResolver = branchScopeResolver;
        _currentUser = currentUser;
        _mediaService = mediaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateAnonymousTemplateLogoResponse>> Handle(
        UpdateAnonymousTemplateLogoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<UpdateAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.Unauthenticated", "Authentication is required.", ErrorType.Security));
        }

        var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == _currentUser.UserId.Value,
            cancellationToken);

        Guid? branchId = null;
        if (!isSuperAdmin)
        {
            var scope = await _branchScopeResolver.ResolveAsync(cancellationToken);
            if (scope.IsFailure)
            {
                return Result<UpdateAnonymousTemplateLogoResponse>.Fail(scope.Errors);
            }
            branchId = scope.Value.BranchId;
        }

        var template = await _templateReadRepository.GetByPropertyAsync(
            x => x.Id == request.AnonymousTemplateId &&
                 (isSuperAdmin || x.BranchId == branchId),
            cancellationToken);

        if (template is null)
        {
            return Result<UpdateAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.TemplateNotFound", "Anonymous template was not found.", ErrorType.NotFound));
        }

        if (template.IsGlobal)
        {
            return Result<UpdateAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.GlobalForbidden", "A global anonymous template cannot have a logo.", ErrorType.Security));
        }

        if (template.SourceGlobalAnonymousTemplateId.HasValue && !isSuperAdmin)
        {
            return Result<UpdateAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.ManagedCopyForbidden", "Only a SuperAdmin can change the logo of a managed global copy.", ErrorType.Security));
        }

        var validationError = TemplateLogoMedia.Validate(request.Logo);
        if (validationError is not null)
        {
            return Result<UpdateAnonymousTemplateLogoResponse>.Fail(validationError);
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

        return Result<UpdateAnonymousTemplateLogoResponse>.Ok(
            new UpdateAnonymousTemplateLogoResponse(template.Id, newLogoPath));
    }
}
