using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

    internal sealed class GetMyOperatorTemplatesQueryHandler
        : IQueryHandler<GetMyOperatorTemplatesQuery, GetMyOperatorTemplatesResponse>
    {
        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
        private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetMyOperatorTemplatesQueryHandler(
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
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

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

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

            var utcNow = DateTime.UtcNow;

            var templates = await _operatorTemplateReadRepository.ListAsync(
                new GetAssignedTemplatesForMyOperatorSpec(
                    currentOperator.OperatorId,
                    utcNow),
                cancellationToken);

            var templateIds = templates
                .Select(x => x.TemplateId)
                .Distinct()
                .ToArray();

            var templateQuestions = await GetTemplateQuestionsAsync(
                templateIds,
                cancellationToken);

            var optionsByQuestionId = await GetOptionsByQuestionIdAsync(
                templateQuestions,
                cancellationToken);

            var questionsByTemplateId = BuildQuestionsByTemplateId(
                templateQuestions,
                optionsByQuestionId);

            var questionConditionsByTemplateId = await GetQuestionConditionsByTemplateIdAsync(
                templateIds,
                cancellationToken);

            var latestResponsesByTemplateId = await GetLatestResponsesByTemplateIdAsync(
                currentOperator.OperatorId,
                templateIds,
                cancellationToken);

            var latestResponseIds = latestResponsesByTemplateId
                .Values
                .Select(x => x.SurveyResponseId)
                .Distinct()
                .ToArray();

            var latestResponsesBySurveyResponseId = latestResponsesByTemplateId
                .Values
                .GroupBy(x => x.SurveyResponseId)
                .ToDictionary(
                    x => x.Key,
                    x => x.First());

            var templateQuestionIdByTemplateAndQuestion = templateQuestions
                .GroupBy(x => new
                {
                    x.TemplateId,
                    x.QuestionId
                })
                .ToDictionary(
                    x => (x.Key.TemplateId, x.Key.QuestionId),
                    x => x
                        .OrderBy(question => question.Order)
                        .First()
                        .TemplateQuestionId);

            var latestAnswersBySurveyResponseId = await GetLatestAnswersBySurveyResponseIdAsync(
                latestResponseIds,
                latestResponsesBySurveyResponseId,
                templateQuestionIdByTemplateAndQuestion,
                cancellationToken);

            var templateItems = templates
                .Select(template =>
                {
                    IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions =
                        questionsByTemplateId.TryGetValue(
                            template.TemplateId,
                            out var templateQuestionsList)
                                ? templateQuestionsList
                                : Array.Empty<MyOperatorTemplateQuestionResponse>();

                    IReadOnlyCollection<TemplateQuestionConditionResponse> rawConditions =
                        questionConditionsByTemplateId.TryGetValue(
                            template.TemplateId,
                            out var templateConditions)
                                ? templateConditions
                                : Array.Empty<TemplateQuestionConditionResponse>();

                    var validConditions = FilterValidConditions(
                        questions,
                        rawConditions);

                    var latestResponse = BuildLatestResponse(
                        template.TemplateId,
                        latestResponsesByTemplateId,
                        latestAnswersBySurveyResponseId,
                        questions,
                        validConditions);

                    return new MyOperatorTemplateItemResponse
                    {
                        TemplateId = template.TemplateId,
                        NameEn = template.NameEn,
                        NameAr = template.NameAr,
                        Description = template.Description,

                        ActiveFrom = template.ActiveFrom,
                        ExpireTo = template.ExpireTo,

                        BranchId = template.BranchId,
                        BranchNameEn = template.BranchNameEn,
                        BranchNameAr = template.BranchNameAr,
                        BranchCode = template.BranchCode,

                        QuestionsCount = questions.Count,

                        HasAnswered = latestResponsesByTemplateId.ContainsKey(template.TemplateId),
                        LatestResponse = latestResponse,

                        Questions = questions,
                        QuestionConditions = validConditions
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

        private async Task<IReadOnlyCollection<TemplateQuestionForMyOperatorDto>>
            GetTemplateQuestionsAsync(
                IReadOnlyCollection<Guid> templateIds,
                CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return Array.Empty<TemplateQuestionForMyOperatorDto>();
            }

            return await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForMyOperatorTemplatesSpec(templateIds),
                cancellationToken);
        }

        private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorQuestionOptionResponse>>>
            GetOptionsByQuestionIdAsync(
                IReadOnlyCollection<TemplateQuestionForMyOperatorDto> templateQuestions,
                CancellationToken cancellationToken)
        {
            var singleChoiceQuestionIds = templateQuestions
                .Where(x => x.Type == QuestionType.SingleChoice.ToString())
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionResponse> questionOptions;

            if (singleChoiceQuestionIds.Length == 0)
            {
                questionOptions = Array.Empty<QuestionOptionResponse>();
            }
            else
            {
                questionOptions = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsByQuestionIdsSpec(singleChoiceQuestionIds),
                    cancellationToken);
            }

            return questionOptions
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
                            Order = option.Order,
                            Value = option.Value
                        })
                        .ToArray());
        }

        private static IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorTemplateQuestionResponse>>
            BuildQuestionsByTemplateId(
                IReadOnlyCollection<TemplateQuestionForMyOperatorDto> templateQuestions,
                IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorQuestionOptionResponse>> optionsByQuestionId)
        {
            return templateQuestions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<MyOperatorTemplateQuestionResponse>)x
                        .OrderBy(question => question.Order)
                        .Select(question =>
                        {
                            IReadOnlyCollection<MyOperatorQuestionOptionResponse> options =
                                question.Type == QuestionType.SingleChoice.ToString()
                                && optionsByQuestionId.TryGetValue(
                                    question.QuestionId,
                                    out var questionOptionsList)
                                        ? questionOptionsList
                                        : Array.Empty<MyOperatorQuestionOptionResponse>();

                            return new MyOperatorTemplateQuestionResponse
                            {
                                TemplateQuestionId = question.TemplateQuestionId,
                                QuestionId = question.QuestionId,
                                Order = question.Order,

                                TextEn = question.TextEn,
                                TextAr = question.TextAr,
                                Type = question.Type,

                                GroupId = question.GroupId,
                                GroupNameEn = question.GroupNameEn,
                                GroupNameAr = question.GroupNameAr,

                                Options = options
                            };
                        })
                        .ToArray());
        }

        private async Task<IReadOnlyDictionary<Guid, IReadOnlyCollection<TemplateQuestionConditionResponse>>>
            GetQuestionConditionsByTemplateIdAsync(
                IReadOnlyCollection<Guid> templateIds,
                CancellationToken cancellationToken)
        {
            if (templateIds.Count == 0)
            {
                return new Dictionary<Guid, IReadOnlyCollection<TemplateQuestionConditionResponse>>();
            }

            var questionConditions = await _conditionReadRepository.ListAsync(
                new GetTemplateQuestionConditionsByTemplateIdsSpec(templateIds),
                cancellationToken);

            return questionConditions
                .GroupBy(x => x.TemplateId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<TemplateQuestionConditionResponse>)x
                        .OrderBy(condition => condition.Order)
                        .ThenBy(condition => condition.ParentTemplateQuestionId)
                        .ThenBy(condition => condition.ChildTemplateQuestionId)
                        .Select(condition => condition.ToResponse())
                        .ToArray());
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
                IReadOnlyDictionary<Guid, LatestSurveyResponseForMyOperatorTemplateDto> latestResponsesBySurveyResponseId,
                IReadOnlyDictionary<(Guid TemplateId, Guid QuestionId), Guid> templateQuestionIdByTemplateAndQuestion,
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
                        .Select(answer =>
                        {
                            Guid? templateQuestionId = null;

                            if (latestResponsesBySurveyResponseId.TryGetValue(
                                    answer.SurveyResponseId,
                                    out var latestResponse)
                                && templateQuestionIdByTemplateAndQuestion.TryGetValue(
                                    (latestResponse.TemplateId, answer.QuestionId),
                                    out var mappedTemplateQuestionId))
                            {
                                templateQuestionId = mappedTemplateQuestionId;
                            }

                            return new MyOperatorTemplateLatestAnswerResponse
                            {
                                TemplateQuestionId = templateQuestionId,
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
                            };
                        })
                        .ToArray());
        }

        private static MyOperatorTemplateLatestResponse? BuildLatestResponse(
            Guid templateId,
            IReadOnlyDictionary<Guid, LatestSurveyResponseForMyOperatorTemplateDto> latestResponsesByTemplateId,
            IReadOnlyDictionary<Guid, IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse>> answersBySurveyResponseId,
            IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions,
            IReadOnlyCollection<TemplateQuestionConditionResponse> conditions)
        {
            if (!latestResponsesByTemplateId.TryGetValue(templateId, out var latestResponse))
            {
                return null;
            }

            IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> allAnswers =
                answersBySurveyResponseId.TryGetValue(
                    latestResponse.SurveyResponseId,
                    out var latestAnswers)
                        ? latestAnswers
                        : Array.Empty<MyOperatorTemplateLatestAnswerResponse>();

            var validConditions = FilterValidConditions(
                questions,
                conditions);

            var orderedVisibleTemplateQuestionIds = CalculateVisibleTemplateQuestionIdsInRenderOrder(
                questions,
                validConditions,
                allAnswers);

            var answersByTemplateQuestionId = allAnswers
                .Where(answer => answer.TemplateQuestionId.HasValue)
                .GroupBy(answer => answer.TemplateQuestionId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            var visibleAnswers = orderedVisibleTemplateQuestionIds
                .Where(answersByTemplateQuestionId.ContainsKey)
                .Select(templateQuestionId => answersByTemplateQuestionId[templateQuestionId])
                .ToArray();

            return new MyOperatorTemplateLatestResponse
            {
                SurveyResponseId = latestResponse.SurveyResponseId,
                SubmittedOnUtc = latestResponse.SubmittedOnUtc,
                AnswersCount = visibleAnswers.Length,
                Score = new MyOperatorTemplateLatestScoreResponse
                {
                    ActualScore = latestResponse.ActualScore,
                    MaxScore = latestResponse.MaxScore,
                    Percentage = latestResponse.ScorePercentage
                },
                Answers = visibleAnswers
            };
        }

        private static Guid[] CalculateVisibleTemplateQuestionIdsInRenderOrder(
            IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions,
            IReadOnlyCollection<TemplateQuestionConditionResponse> conditions,
            IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> answers)
        {
            var validConditions = FilterValidConditions(
                questions,
                conditions);

            var questionsByTemplateQuestionId = questions
                .ToDictionary(
                    question => question.TemplateQuestionId,
                    question => question);

            var childTemplateQuestionIds = validConditions
                .Select(condition => condition.ChildTemplateQuestionId)
                .ToHashSet();

            var rootTemplateQuestionIds = questions
                .Where(question => !childTemplateQuestionIds.Contains(question.TemplateQuestionId))
                .OrderBy(question => question.Order)
                .ThenBy(question => question.TemplateQuestionId)
                .Select(question => question.TemplateQuestionId)
                .ToArray();

            var answersByTemplateQuestionId = answers
                .Where(answer => answer.TemplateQuestionId.HasValue)
                .GroupBy(answer => answer.TemplateQuestionId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            var conditionsByParentTemplateQuestionId = validConditions
                .GroupBy(condition => condition.ParentTemplateQuestionId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(condition => condition.Order)
                        .ThenBy(condition =>
                            questionsByTemplateQuestionId.TryGetValue(
                                condition.ChildTemplateQuestionId,
                                out var childQuestion)
                                    ? childQuestion.Order
                                    : int.MaxValue)
                        .ThenBy(condition => condition.ChildTemplateQuestionId)
                        .ToArray());

            var orderedVisibleTemplateQuestionIds = new List<Guid>();
            var visitedTemplateQuestionIds = new HashSet<Guid>();

            foreach (var rootTemplateQuestionId in rootTemplateQuestionIds)
            {
                AddVisibleQuestionAndTriggeredChildren(
                    templateQuestionId: rootTemplateQuestionId,
                    questionsByTemplateQuestionId: questionsByTemplateQuestionId,
                    answersByTemplateQuestionId: answersByTemplateQuestionId,
                    conditionsByParentTemplateQuestionId: conditionsByParentTemplateQuestionId,
                    orderedVisibleTemplateQuestionIds: orderedVisibleTemplateQuestionIds,
                    visitedTemplateQuestionIds: visitedTemplateQuestionIds);
            }

            return orderedVisibleTemplateQuestionIds.ToArray();
        }

        private static void AddVisibleQuestionAndTriggeredChildren(
            Guid templateQuestionId,
            IReadOnlyDictionary<Guid, MyOperatorTemplateQuestionResponse> questionsByTemplateQuestionId,
            IReadOnlyDictionary<Guid, MyOperatorTemplateLatestAnswerResponse> answersByTemplateQuestionId,
            IReadOnlyDictionary<Guid, TemplateQuestionConditionResponse[]> conditionsByParentTemplateQuestionId,
            List<Guid> orderedVisibleTemplateQuestionIds,
            HashSet<Guid> visitedTemplateQuestionIds)
        {
            if (!questionsByTemplateQuestionId.ContainsKey(templateQuestionId))
            {
                return;
            }

            if (!visitedTemplateQuestionIds.Add(templateQuestionId))
            {
                return;
            }

            orderedVisibleTemplateQuestionIds.Add(templateQuestionId);

            if (!answersByTemplateQuestionId.TryGetValue(
                    templateQuestionId,
                    out var parentAnswer))
            {
                return;
            }

            if (!conditionsByParentTemplateQuestionId.TryGetValue(
                    templateQuestionId,
                    out var childConditions))
            {
                return;
            }

            foreach (var condition in childConditions)
            {
                if (!questionsByTemplateQuestionId.ContainsKey(condition.ChildTemplateQuestionId))
                {
                    continue;
                }

                if (!ConditionMatchesAnswer(condition, parentAnswer))
                {
                    continue;
                }

                AddVisibleQuestionAndTriggeredChildren(
                    templateQuestionId: condition.ChildTemplateQuestionId,
                    questionsByTemplateQuestionId: questionsByTemplateQuestionId,
                    answersByTemplateQuestionId: answersByTemplateQuestionId,
                    conditionsByParentTemplateQuestionId: conditionsByParentTemplateQuestionId,
                    orderedVisibleTemplateQuestionIds: orderedVisibleTemplateQuestionIds,
                    visitedTemplateQuestionIds: visitedTemplateQuestionIds);
            }
        }

        private static TemplateQuestionConditionResponse[] FilterValidConditions(
            IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions,
            IReadOnlyCollection<TemplateQuestionConditionResponse> conditions)
        {
            if (questions.Count == 0 || conditions.Count == 0)
            {
                return Array.Empty<TemplateQuestionConditionResponse>();
            }

            var existingTemplateQuestionIds = questions
                .Select(question => question.TemplateQuestionId)
                .ToHashSet();

            return conditions
                .Where(condition =>
                    existingTemplateQuestionIds.Contains(condition.ParentTemplateQuestionId) &&
                    existingTemplateQuestionIds.Contains(condition.ChildTemplateQuestionId))
                .OrderBy(condition => condition.Order)
                .ThenBy(condition => condition.ParentTemplateQuestionId)
                .ThenBy(condition => condition.ChildTemplateQuestionId)
                .ToArray();
        }

        private static HashSet<Guid> CalculateVisibleTemplateQuestionIds(
            IReadOnlyCollection<MyOperatorTemplateQuestionResponse> questions,
            IReadOnlyCollection<TemplateQuestionConditionResponse> conditions,
            IReadOnlyCollection<MyOperatorTemplateLatestAnswerResponse> answers)
        {
            var validConditions = FilterValidConditions(
                questions,
                conditions);

            var existingTemplateQuestionIds = questions
                .Select(question => question.TemplateQuestionId)
                .ToHashSet();

            var childTemplateQuestionIds = validConditions
                .Select(condition => condition.ChildTemplateQuestionId)
                .ToHashSet();

            var rootTemplateQuestionIds = questions
                .Where(question => !childTemplateQuestionIds.Contains(question.TemplateQuestionId))
                .OrderBy(question => question.Order)
                .Select(question => question.TemplateQuestionId)
                .ToArray();

            var answersByTemplateQuestionId = answers
                .Where(answer => answer.TemplateQuestionId.HasValue)
                .GroupBy(answer => answer.TemplateQuestionId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.First());

            var conditionsByParentTemplateQuestionId = validConditions
                .GroupBy(condition => condition.ParentTemplateQuestionId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderBy(condition => condition.Order)
                        .ThenBy(condition => condition.ChildTemplateQuestionId)
                        .ToArray());

            var visibleTemplateQuestionIds = new HashSet<Guid>(rootTemplateQuestionIds);
            var queue = new Queue<Guid>(rootTemplateQuestionIds);

            while (queue.Count > 0)
            {
                var parentTemplateQuestionId = queue.Dequeue();

                if (!answersByTemplateQuestionId.TryGetValue(
                        parentTemplateQuestionId,
                        out var parentAnswer))
                {
                    continue;
                }

                if (!conditionsByParentTemplateQuestionId.TryGetValue(
                        parentTemplateQuestionId,
                        out var parentConditions))
                {
                    continue;
                }

                foreach (var condition in parentConditions)
                {
                    if (!existingTemplateQuestionIds.Contains(condition.ChildTemplateQuestionId))
                    {
                        continue;
                    }

                    if (!ConditionMatchesAnswer(condition, parentAnswer))
                    {
                        continue;
                    }

                    if (visibleTemplateQuestionIds.Add(condition.ChildTemplateQuestionId))
                    {
                        queue.Enqueue(condition.ChildTemplateQuestionId);
                    }
                }
            }

            return visibleTemplateQuestionIds;
        }

        private static bool ConditionMatchesAnswer(
            TemplateQuestionConditionResponse condition,
            MyOperatorTemplateLatestAnswerResponse answer)
        {
            return condition.TriggerType switch
            {
                1 => answer.SelectedQuestionOptionId.HasValue
                     && condition.SelectedQuestionOptionId.HasValue
                     && answer.SelectedQuestionOptionId.Value == condition.SelectedQuestionOptionId.Value,

                2 => answer.StarRatingValue.HasValue
                     && condition.TriggerValue.HasValue
                     && answer.StarRatingValue.Value == condition.TriggerValue.Value,

                3 => answer.SmileValue.HasValue
                     && condition.TriggerValue.HasValue
                     && answer.SmileValue.Value == condition.TriggerValue.Value,

                _ => false
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