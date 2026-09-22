using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Application.Shared.Media;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplateLogo;

internal sealed class DeleteAnonymousTemplateLogoCommandHandler
    : ICommandHandler<DeleteAnonymousTemplateLogoCommand, DeleteAnonymousTemplateLogoResponse>
{
    private readonly IWriteReadRepository<AnonymousTemplate> _templateReadRepository;
    private readonly IWriteRepository<AnonymousTemplate> _templateWriteRepository;
    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly ICurrentBranchScopeResolver _branchScopeResolver;
    private readonly ICurrentUser _currentUser;
    private readonly IMediaService _mediaService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnonymousTemplateLogoCommandHandler(
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

    public async Task<Result<DeleteAnonymousTemplateLogoResponse>> Handle(
        DeleteAnonymousTemplateLogoCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<DeleteAnonymousTemplateLogoResponse>.Fail(new Error(
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
                return Result<DeleteAnonymousTemplateLogoResponse>.Fail(scope.Errors);
            }
            branchId = scope.Value.BranchId;
        }

        var template = await _templateReadRepository.GetByPropertyAsync(
            x => x.Id == request.AnonymousTemplateId &&
                 (isSuperAdmin || x.BranchId == branchId),
            cancellationToken);

        if (template is null)
        {
            return Result<DeleteAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.TemplateNotFound", "Anonymous template was not found.", ErrorType.NotFound));
        }

        if (template.IsGlobal)
        {
            return Result<DeleteAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.GlobalForbidden", "A global anonymous template cannot have a logo.", ErrorType.Security));
        }

        if (template.SourceGlobalAnonymousTemplateId.HasValue && !isSuperAdmin)
        {
            return Result<DeleteAnonymousTemplateLogoResponse>.Fail(new Error(
                "AnonymousTemplates.Logo.ManagedCopyForbidden", "Only a SuperAdmin can change the logo of a managed global copy.", ErrorType.Security));
        }

        var oldLogoPath = template.LogoPath;
        template.ClearLogo();
        _templateWriteRepository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(oldLogoPath))
        {
            _mediaService.Remove(TemplateLogoMedia.ToPhysicalPath(oldLogoPath));
        }

        return Result<DeleteAnonymousTemplateLogoResponse>.Ok(
            new DeleteAnonymousTemplateLogoResponse(template.Id, template.LogoPath));
    }
}
