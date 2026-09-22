using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.RestoreAnonymousTemplate
{
    internal sealed class RestoreAnonymousTemplateCommandHandler
        : ICommandHandler<RestoreAnonymousTemplateCommand, RestoreAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreAnonymousTemplateCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateWriteRepository = anonymousTemplateWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreAnonymousTemplateResponse>> Handle(
            RestoreAnonymousTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var isSuperAdmin = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            Guid? currentBranchId = null;

            if (!isSuperAdmin)
            {
                var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                    cancellationToken);

                if (currentBranchScope.IsFailure)
                {
                    return Result<RestoreAnonymousTemplateResponse>.Fail(currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForRestoreSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<RestoreAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Restore.TemplateNotFound",
                    Message: ErrorMessage.RestoreAnonymousTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if ((anonymousTemplate.IsGlobal && !anonymousTemplate.IsArchived) ||
                (anonymousTemplate.IsBranchScoped && anonymousTemplate.IsActive))
            {
                return Result<RestoreAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Restore.TemplateAlreadyActive",
                    Message: ErrorMessage.RestoreAnonymousTemplate_Template_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            anonymousTemplate.Restore();

            _anonymousTemplateWriteRepository.Update(anonymousTemplate);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<RestoreAnonymousTemplateResponse>.Ok(
                MapToResponse(anonymousTemplate));
        }

        private static RestoreAnonymousTemplateResponse MapToResponse(
            AnonymousTemplate anonymousTemplate)
        {
            return new RestoreAnonymousTemplateResponse
            {
                AnonymousTemplateId = anonymousTemplate.Id,
                BranchId = anonymousTemplate.BranchId,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                IsActive = anonymousTemplate.IsActive,
                IsArchived = anonymousTemplate.IsArchived
            };
        }
    }
}
