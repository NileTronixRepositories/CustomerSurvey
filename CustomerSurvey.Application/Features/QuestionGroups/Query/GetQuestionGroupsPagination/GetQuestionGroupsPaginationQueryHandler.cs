using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
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
        private readonly IWriteReadRepository<ApplicationUser> _applicationUserReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetQuestionGroupsPaginationQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<ApplicationUser> applicationUserReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _applicationUserReadRepository = applicationUserReadRepository
                ?? throw new ArgumentNullException(nameof(applicationUserReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

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

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<Pagination<QuestionGroupPaginationItemResponse>>.Fail(
                    currentBranchScope.Errors);
            }

            var spec = new GetQuestionGroupsPaginationSpec(
                branchId: currentBranchScope.Value.BranchId,
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

    }
}
