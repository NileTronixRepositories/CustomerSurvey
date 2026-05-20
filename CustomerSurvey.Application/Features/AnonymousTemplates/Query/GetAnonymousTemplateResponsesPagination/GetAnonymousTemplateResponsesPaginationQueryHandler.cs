using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateResponsesPagination
{
    internal sealed class GetAnonymousTemplateResponsesPaginationQueryHandler
        : IQueryHandler<GetAnonymousTemplateResponsesPaginationQuery, Pagination<AnonymousTemplateResponsePaginationItemResponse>>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousSurveyResponse> _anonymousSurveyResponseReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetAnonymousTemplateResponsesPaginationQueryHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousSurveyResponse> anonymousSurveyResponseReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _anonymousSurveyResponseReadRepository = anonymousSurveyResponseReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>> Handle(
            GetAnonymousTemplateResponsesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponsesPagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

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
                    return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(new Error(
                        Code: "AnonymousTemplates.ResponsesPagination.CurrentActorNotFound",
                        Message: ErrorMessage.GetAnonymousTemplateResponsesPagination_CurrentActor_NotFound,
                        Type: ErrorType.Security));
                }

                currentBranchId = currentBranchActor.BranchId;
            }

            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForResponsesPaginationSpec(
                    request.AnonymousTemplateId,
                    isSuperAdmin,
                    currentBranchId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Fail(new Error(
                    Code: "AnonymousTemplates.ResponsesPagination.TemplateNotFound",
                    Message: ErrorMessage.GetAnonymousTemplateResponsesPagination_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var spec = new GetAnonymousTemplateResponsesPaginationSpec(request);

            var (items, totalCount) = await _anonymousSurveyResponseReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<AnonymousTemplateResponsePaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<AnonymousTemplateResponsePaginationItemResponse>>.Ok(response);
        }

        private async Task<CurrentBranchActorForGetAnonymousTemplateResponsesPaginationDto?> ResolveBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForAnonymousTemplateResponsesPaginationSpec(applicationUserId),
                cancellationToken);

            if (currentBranchAdmin is not null)
            {
                return currentBranchAdmin;
            }

            var currentBranchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForAnonymousTemplateResponsesPaginationSpec(applicationUserId),
                cancellationToken);

            return currentBranchUser;
        }
    }
}