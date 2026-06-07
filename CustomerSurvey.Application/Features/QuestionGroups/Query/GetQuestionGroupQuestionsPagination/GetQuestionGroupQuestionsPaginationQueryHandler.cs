using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupQuestionsPagination
{
    internal sealed class GetQuestionGroupQuestionsPaginationQueryHandler
        : IQueryHandler<GetQuestionGroupQuestionsPaginationQuery, Pagination<QuestionByGroupPaginationItemResponse>>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetQuestionGroupQuestionsPaginationQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<QuestionByGroupPaginationItemResponse>>> Handle(
            GetQuestionGroupQuestionsPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<QuestionByGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "QuestionGroups.QuestionsPagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
                x => x.ApplicationUserId == currentApplicationUserId,
                cancellationToken);

            Guid? currentBranchId = null;

            if (!currentSuperAdminExists)
            {
                var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                    cancellationToken);

                if (currentBranchScope.IsFailure)
                {
                    return Result<Pagination<QuestionByGroupPaginationItemResponse>>.Fail(
                        currentBranchScope.Errors);
                }

                currentBranchId = currentBranchScope.Value.BranchId;
            }

            var questionGroup = await _questionGroupReadRepository.FirstOrDefaultAsync(
                new GetQuestionGroupForQuestionsPaginationSpec(
                    request.QuestionGroupId,
                    currentBranchId),
                cancellationToken);

            if (questionGroup is null)
            {
                return Result<Pagination<QuestionByGroupPaginationItemResponse>>.Fail(new Error(
                    Code: "QuestionGroups.QuestionsPagination.QuestionGroupNotFound",
                    Message: ErrorMessage.DeleteQuestionGroup_QuestionGroup_NotFound,
                    Type: ErrorType.NotFound));
            }

            var spec = new GetQuestionGroupQuestionsPaginationSpec(
                questionGroup.Id,
                questionGroup.BranchId,
                request);

            var (items, totalCount) = await _questionReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var response = new Pagination<QuestionByGroupPaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: items);

            return Result<Pagination<QuestionByGroupPaginationItemResponse>>.Ok(response);
        }

    }
}
