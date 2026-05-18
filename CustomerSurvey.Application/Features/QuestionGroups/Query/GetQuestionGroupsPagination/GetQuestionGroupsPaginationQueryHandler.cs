using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsPagination
{
    internal sealed class GetQuestionGroupsPaginationQueryHandler
        : IQueryHandler<GetQuestionGroupsPaginationQuery, Pagination<QuestionGroupPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetQuestionGroupsPaginationQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<QuestionGroupPaginationItemResponse>>> Handle(
            GetQuestionGroupsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<QuestionGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "QuestionGroups.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchId = await ResolveActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (!actorBranchId.HasValue)
            {
                return Result<Pagination<QuestionGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "QuestionGroups.Pagination.CurrentBranchActorNotFound",
                    Message: ErrorMessage.GetQuestionGroupsPagination_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var spec = new GetQuestionGroupsPaginationSpec(
                branchId: actorBranchId.Value,
                searchParameters: request);

            var (items, totalCount) = await _questionGroupReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var creatorApplicationUserIds = items
                .Select(x => x.CreatedByApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionGroupPaginationCreatorDto> creators;

            if (creatorApplicationUserIds.Length == 0)
            {
                creators = Array.Empty<QuestionGroupPaginationCreatorDto>();
            }
            else
            {
                creators = await _applicationUserReadRepository.ListAsync(
                    new GetQuestionGroupCreatorsForQuestionGroupsPaginationSpec(creatorApplicationUserIds),
                    cancellationToken);
            }

            var creatorsByApplicationUserId = creators.ToDictionary(
                x => x.ApplicationUserId,
                x => x);

            var responseItems = items
                .Select(group =>
                {
                    creatorsByApplicationUserId.TryGetValue(
                        group.CreatedByApplicationUserId,
                        out var creator);

                    return new QuestionGroupPaginationItemResponse
                    {
                        GroupId = group.GroupId,
                        BranchId = group.BranchId,
                        NameEn = group.NameEn,
                        NameAr = group.NameAr,
                        IsActive = group.IsActive,
                        QuestionsCount = group.QuestionsCount,
                        CreatedBy = creator is null
                            ? null
                            : new QuestionGroupPaginationCreatedByResponse
                            {
                                NameEn = creator.NameEn,
                                NameAr = creator.NameAr
                            },
                        CreatedOnUtc = group.CreatedOnUtc
                    };
                })
                .ToArray();

            var response = new Pagination<QuestionGroupPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

            return Result<Pagination<QuestionGroupPaginationItemResponse>>.Ok(response);
        }

        private async Task<Guid?> ResolveActorBranchIdAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin.BranchId;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForQuestionGroupSpec(applicationUserId),
                cancellationToken);

            return branchUser?.BranchId;
        }
    }
}