using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails
{
    internal sealed class GetGlobalQuestionDetailsQueryHandler
        : IQueryHandler<GetGlobalQuestionDetailsQuery, GetGlobalQuestionDetailsResponse>
    {
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetGlobalQuestionDetailsQueryHandler(
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SuperAdmin> superAdminReadRepository,
            ICurrentUser currentUser)
        {
            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _superAdminReadRepository = superAdminReadRepository
                ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetGlobalQuestionDetailsResponse>> Handle(
            GetGlobalQuestionDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetGlobalQuestionDetailsResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Details.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentSuperAdmin = await _superAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentSuperAdminForGetGlobalQuestionDetailsSpec(
                    currentApplicationUserId),
                cancellationToken);

            if (currentSuperAdmin is null)
            {
                return Result<GetGlobalQuestionDetailsResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Details.CurrentSuperAdminNotFound",
                    Message: ErrorMessage.GetGlobalQuestionDetails_CurrentSuperAdmin_NotFound,
                    Type: ErrorType.Security));
            }

            var question = await _questionReadRepository.FirstOrDefaultAsync(
                new GetGlobalQuestionDetailsSpec(request.QuestionId),
                cancellationToken);

            if (question is null)
            {
                return Result<GetGlobalQuestionDetailsResponse>.Fail(new Error(
                    Code: "GlobalQuestions.Details.QuestionNotFound",
                    Message: ErrorMessage.GetGlobalQuestionDetails_Question_NotFound,
                    Type: ErrorType.NotFound));
            }

            IReadOnlyCollection<QuestionOptionResponse> options;

            if (question.Type != QuestionType.SingleChoice)
            {
                options = Array.Empty<QuestionOptionResponse>();
            }
            else
            {
                options = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsByQuestionIdsSpec(
                        new[] { question.QuestionId }),
                    cancellationToken);
            }

            var response = question with
            {
                Options = options
                    .OrderBy(x => x.Order)
                    .ToArray()
            };

            return Result<GetGlobalQuestionDetailsResponse>.Ok(response);
        }
    }
}