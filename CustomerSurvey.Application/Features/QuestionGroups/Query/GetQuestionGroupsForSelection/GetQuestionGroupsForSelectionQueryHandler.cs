using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    internal sealed class GetQuestionGroupsForSelectionQueryHandler
         : IQueryHandler<GetQuestionGroupsForSelectionQuery, IReadOnlyCollection<QuestionGroupSelectionResponse>>
    {
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetQuestionGroupsForSelectionQueryHandler(
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<QuestionGroupSelectionResponse>>> Handle(
            GetQuestionGroupsForSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<QuestionGroupSelectionResponse>>.Fail(new Error(
                    Code: "QuestionGroups.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<IReadOnlyCollection<QuestionGroupSelectionResponse>>.Fail(
                    currentBranchScope.Errors);
            }

            var spec = new GetQuestionGroupsForSelectionSpec(currentBranchScope.Value.BranchId);

            var groups = await _questionGroupReadRepository.ListAsync(
                spec,
                cancellationToken);

            return Result<IReadOnlyCollection<QuestionGroupSelectionResponse>>.Ok(groups);
        }

    }
}
