using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplatesPagination
{
    internal sealed class GetAnonymousTemplatesPaginationQueryHandler
        : IQueryHandler<GetAnonymousTemplatesPaginationQuery, Pagination<AnonymousTemplatePaginationItemResponse>>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplatesPaginationQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<AnonymousTemplatePaginationItemResponse>>> Handle(
            GetAnonymousTemplatesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<AnonymousTemplatePaginationItemResponse>>.Fail(new Error(
                    Code: "AnonymousTemplates.Pagination.Unauthenticated",
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
                var currentBranchActor = await ResolveBranchActorAsync(
                    currentApplicationUserId,
                    cancellationToken);

                if (currentBranchActor is null)
                {
                    return Result<Pagination<AnonymousTemplatePaginationItemResponse>>.Fail(new Error(
                        Code: "AnonymousTemplates.Pagination.CurrentActorNotFound",
                        Message: ErrorMessage.GetAnonymousTemplatesPagination_CurrentActor_NotFound,
                        Type: ErrorType.Security));
                }

                currentBranchId = currentBranchActor.BranchId;
            }

            var spec = new GetAnonymousTemplatesPaginationSpec(
                request,
                isSuperAdmin,
                currentBranchId);

            var (items, totalCount) = await _anonymousTemplateReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<AnonymousTemplatePaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<AnonymousTemplatePaginationItemResponse>>.Ok(response);
        }

        private async Task<CurrentBranchActorForGetAnonymousTemplatesPaginationDto?> ResolveBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForGetAnonymousTemplatesPaginationSpec(applicationUserId),
                cancellationToken);

            if (currentBranchAdmin is not null)
            {
                return currentBranchAdmin;
            }

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForGetAnonymousTemplatesPaginationSpec(applicationUserId),
                cancellationToken);

            return currentBranchUser;
        }
    }
}