using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.DeleteAnonymousTemplate
{
    internal sealed class DeleteAnonymousTemplateCommandHandler
        : ICommandHandler<DeleteAnonymousTemplateCommand, DeleteAnonymousTemplateResponse>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteRepository<AnonymousTemplate> _anonymousTemplateWriteRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAnonymousTemplateCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteRepository<AnonymousTemplate> anonymousTemplateWriteRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousTemplateWriteRepository = anonymousTemplateWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateWriteRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteAnonymousTemplateResponse>> Handle(
            DeleteAnonymousTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Delete.Unauthenticated",
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
                var branchActor = await ResolveBranchActorAsync(
                    currentApplicationUserId,
                    cancellationToken);

                if (branchActor is null)
                {
                    return Result<DeleteAnonymousTemplateResponse>.Fail(new Error(
                        Code: "AnonymousTemplates.Delete.CurrentActorNotFound",
                        Message: ErrorMessage.DeleteAnonymousTemplate_CurrentActor_NotFound,
                        Type: ErrorType.Security));
                }

                currentBranchId = branchActor.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForDeleteSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<DeleteAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Delete.TemplateNotFound",
                    Message: ErrorMessage.DeleteAnonymousTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!anonymousTemplate.IsActive)
            {
                return Result<DeleteAnonymousTemplateResponse>.Fail(new Error(
                    Code: "AnonymousTemplates.Delete.TemplateAlreadyInactive",
                    Message: ErrorMessage.DeleteAnonymousTemplate_Template_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            anonymousTemplate.Deactivate();

            _anonymousTemplateWriteRepository.Update(anonymousTemplate);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<DeleteAnonymousTemplateResponse>.Ok(
                MapToResponse(anonymousTemplate));
        }

        private async Task<CurrentBranchActorForDeleteAnonymousTemplateDto?> ResolveBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForDeleteAnonymousTemplateSpec(applicationUserId),
                cancellationToken);

            if (currentBranchAdmin is not null)
            {
                return currentBranchAdmin;
            }

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForDeleteAnonymousTemplateSpec(applicationUserId),
                cancellationToken);

            return currentBranchUser;
        }

        private static DeleteAnonymousTemplateResponse MapToResponse(
            AnonymousTemplate anonymousTemplate)
        {
            return new DeleteAnonymousTemplateResponse
            {
                AnonymousTemplateId = anonymousTemplate.Id,
                BranchId = anonymousTemplate.BranchId,
                Scope = anonymousTemplate.Scope,
                ScopeName = anonymousTemplate.Scope.ToString(),
                IsGlobal = anonymousTemplate.Scope == AnonymousTemplateScope.Global,
                Status = anonymousTemplate.Status,
                StatusName = anonymousTemplate.Status.ToString(),
                IsActive = anonymousTemplate.IsActive
            };
        }
    }
}