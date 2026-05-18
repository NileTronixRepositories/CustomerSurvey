using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection
{
    internal sealed class GetGlobalQuestionGroupsSelectionQueryHandler
        : IQueryHandler<GetGlobalQuestionGroupsSelectionQuery, IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetGlobalQuestionGroupsSelectionQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>> Handle(
            GetGlobalQuestionGroupsSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForGlobalQuestionGroupsSelectionSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>.Fail(new Error(
                    Code: "GlobalQuestionGroups.Selection.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetGlobalQuestionGroupsSelection_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var groups = await _questionGroupReadRepository.ListAsync(
                new GetGlobalQuestionGroupsSelectionSpec(),
                cancellationToken);

            return Result<IReadOnlyCollection<GlobalQuestionGroupSelectionResponse>>.Ok(groups);
        }
    }
}