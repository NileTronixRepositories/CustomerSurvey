using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    using DomainOperator = Domain.Identity.Operator;

    internal sealed class GetMyOperatorTemplatesQueryHandler
        : IQueryHandler<GetMyOperatorTemplatesQuery, GetMyOperatorTemplatesResponse>
    {
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
        private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetMyOperatorTemplatesQueryHandler(
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
            IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
            ICurrentUser currentUser)
        {
            _operatorReadRepository = operatorReadRepository
                ?? throw new ArgumentNullException(nameof(operatorReadRepository));

            _operatorTemplateReadRepository = operatorTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(operatorTemplateReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _surveyResponseReadRepository = surveyResponseReadRepository
                ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

            _surveyAnswerReadRepository = surveyAnswerReadRepository
                ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetMyOperatorTemplatesResponse>> Handle(
            GetMyOperatorTemplatesQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetMyOperatorTemplatesResponse>.Fail(new Error(
                    Code: "Operators.MyTemplates.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentOperator = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetCurrentOperatorForMyTemplatesSpec(currentApplicationUserId),
                cancellationToken);

            if (currentOperator is null)
            {
                return Result<GetMyOperatorTemplatesResponse>.Fail(new Error(
                    Code: "Operators.MyTemplates.CurrentOperatorNotFound",
                    Message: ErrorMessage.GetMyOperatorTemplates_CurrentOperator_NotFound,
                    Type: ErrorType.NotFound));
            }

            var templates = await _operatorTemplateReadRepository.ListAsync(
                new GetAssignedTemplatesForMyOperatorSpec(currentOperator.OperatorId),
                cancellationToken);

            var templateIds = templates
                .Select(x => x.TemplateId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<TemplateQuestionForMyOperatorDto> templateQuestions;

            if (templateIds.Length == 0)
            {
                templateQuestions = Array.Empty<TemplateQuestionForMyOperatorDto>();
            }
            else
            {
                templateQuestions = await _templateQuestionReadRepository.ListAsync(
                    new GetTemplateQuestionsForMyOperatorTemplatesSpec(templateIds),
                    cancellationToken);
            }

            var questionIds = templateQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionResponse> questionOptions;

            if (questionIds.Length == 0)
            {
                questionOptions = Array.Empty<QuestionOptionResponse>();
            }
            else
            {
                questionOptions = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsByQuestionIdsSpec(questionIds),
                    cancellationToken);
            }

            var latestResponsesByTemplateId = await GetLatestResponsesByTemplateIdAsync(
                currentOperator.OperatorId,
                templateIds,
                cancellationToken);

            var latestResponseIds = latestResponsesByTemplateId
                .Values
                .Select(x => x.SurveyResponseId)
                .Distinct()
                .ToArray();

            var latestAnswersBySurveyResponseId = await GetLatestAnswersBySurveyResponseIdAsync(
                latestResponseIds,
                cancellationToken);

            var optionsByQuestionId = questionOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorQuestionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .Select(option => new MyOperatorQuestionOptionResponse
                        {
                            OptionId = option.OptionId,
                            TextEn = option.TextEn,
                            TextAr = option.TextAr,
                            Order = option.Order
                        })
                        .ToArray());

            var questionsByTemplateId = templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorTemplateQuestionResponse>)x
                        .OrderBy(q => q.Order)
                        .Select(q =>
                        {
                            IReadOnlyCollection<MyOperatorQuestionOptionResponse> options =
                                q.Type == QuestionType.SingleChoice.ToString()
                                && optionsByQuestionId.TryGetValue(
                                    q.QuestionId,
                                    out var questionOptionsList)
                                    ? questionOptionsList
                                    : Array.Empty<MyOperatorQuestionOptionResponse>();

                            return new MyOperatorTemplateQuestionResponse
                            {
                                TemplateQuestionId = q.TemplateQuestionId,
                                QuestionId = q.QuestionId,
                                Order = q.Order,
                                TextEn = q.TextEn,
                                TextAr = q.TextAr,
                                Type = q.Type,
                                GroupId = q.GroupId,
                                GroupNameEn = q.GroupNameEn,
                                GroupNameAr = q.GroupNameAr,
                                Options = options
                            };
                        })
                        .ToArray());

            var templateItems = templates
                .Select(template =>
                {
                    IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions =
                        questionsByTemplateId.TryGetValue(
                            template.TemplateId,
                            out var templateQuestionsList)
                                ? templateQuestionsList
                                : Array.Empty<MyOperatorTemplateQuestionResponse>();

                    var latestResponse = BuildLatestResponse(
                        template.TemplateId,
                        latestResponsesByTemplateId,
                        latestAnswersBySurveyResponseId);

                    return new MyOperatorTemplateItemResponse
                    {
                        TemplateId = template.TemplateId,
                        NameEn = template.NameEn,
                        NameAr = template.NameAr,
                        Description = template.Description,
                        BranchId = template.BranchId,
                        BranchNameEn = template.BranchNameEn,
                        BranchNameAr = template.BranchNameAr,
                        BranchCode = template.BranchCode,
                        QuestionsCount = questions.Count,
                        HasAnswered = latestResponse is not null,
                        LatestResponse = latestResponse,
                        Questions = questions
                    };
                })
                .ToArray();

            var response = new GetMyOperatorTemplatesResponse
            {
                OperatorId = currentOperator.OperatorId,
                DepartmentId = currentOperator.DepartmentId,
                TemplatesCount = templateItems.Length,
                Templates = templateItems
            };

            return Result<GetMyOperatorTemplatesResponse>.Ok(response);
        }

        private async Task<IReadOnlyDictionary<Guid, LatestSurveyResponseForMyOperatorTemplateDto>>
            GetLatestResponsesByTemplateIdAsync(
                Guid operatorId,
                IReadOnlyCollection<Guid> templateIds,
                CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return new Dictionary<Guid, LatestSurveyResponseForMyOperatorTemplateDto>();
            }

            var surveyResponses = await _surveyResponseReadRepository.ListAsync(
                new GetLatestSurveyResponsesForMyOperatorTemplatesSpec(
                    operatorId,
                    templateIds),
                cancellationToken);

            return surveyResponses
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(response => response.SubmittedOnUtc)
                        .First());
        }

        private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse>>>
            GetLatestAnswersBySurveyResponseIdAsync(
                IReadOnlyCollection<Guid> latestResponseIds,
                CancellationToken cancellationToken)
        {
            if (latestResponseIds.Count == 0)
            {
                return new Dictionary<Guid, IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse>>();
            }

            var answers = await _surveyAnswerReadRepository.ListAsync(
                new GetLatestSurveyAnswersForMyOperatorTemplatesSpec(latestResponseIds),
                cancellationToken);

            return answers
                .GroupBy(x => x.SurveyResponseId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse>)x
                        .Select(answer => new MyOperatorTemplateLatestAnswerResponse
                        {
                            QuestionId = answer.QuestionId,
                            QuestionType = answer.QuestionType.ToString(),
                            SelectedQuestionOptionId = answer.SelectedQuestionOptionId,
                            SelectedOptionTextEn = answer.SelectedOptionTextEn,
                            SelectedOptionTextAr = answer.SelectedOptionTextAr,
                            StarRatingValue = answer.StarRatingValue,
                            SmileValue = answer.SmileValue,
                            TextAnswer = answer.TextAnswer,
                            VoiceFileName = answer.VoiceFileName,
                            VoiceFileUrl = BuildVoiceFileUrl(answer.VoiceFileName)
                        })
                        .ToArray());
        }

        private static MyOperatorTemplateLatestResponse? BuildLatestResponse(
            Guid templateId,
            IReadOnlyDictionary<Guid, LatestSurveyResponseForMyOperatorTemplateDto> latestResponsesByTemplateId,
            IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse>> answersBySurveyResponseId)
        {
            if (!latestResponsesByTemplateId.TryGetValue(templateId, out var latestResponse))
            {
                return null;
            }

            IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> answers =
                answersBySurveyResponseId.TryGetValue(
                    latestResponse.SurveyResponseId,
                    out var latestAnswers)
                        ? latestAnswers
                        : Array.Empty<MyOperatorTemplateLatestAnswerResponse>();

            return new MyOperatorTemplateLatestResponse
            {
                SurveyResponseId = latestResponse.SurveyResponseId,
                SubmittedOnUtc = latestResponse.SubmittedOnUtc,
                AnswersCount = answers.Count,
                Answers = answers
            };
        }

        private static string? BuildVoiceFileUrl(string? voiceFileName)
        {
            if (string.IsNullOrWhiteSpace(voiceFileName))
            {
                return null;
            }

            return $"Media/{FileNames.SurveyVoiceAnswers}/{voiceFileName}";
        }
    }
}