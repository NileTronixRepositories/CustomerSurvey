using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;
using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed class SubmitOperatorTemplateResponseCommandHandler
        : ICommandHandler<SubmitOperatorTemplateResponseCommand, SubmitOperatorTemplateResponseResponse>
    {
        private const long MaxVoiceFileSizeInBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedVoiceExtensions =
        [
            ".mp3",
            ".wav",
            ".m4a",
            ".aac",
            ".ogg"
        ];

        private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteRepository<SurveyResponse> _surveyResponseWriteRepository;
        private readonly IMediaService _mediaService;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitOperatorTemplateResponseCommandHandler(
            IWriteReadRepository<DomainOperator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
            IWriteRepository<SurveyResponse> surveyResponseWriteRepository,
            IMediaService mediaService,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
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

            _surveyResponseWriteRepository = surveyResponseWriteRepository
                ?? throw new ArgumentNullException(nameof(surveyResponseWriteRepository));

            _mediaService = mediaService
                ?? throw new ArgumentNullException(nameof(mediaService));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<SubmitOperatorTemplateResponseResponse>> Handle(
            SubmitOperatorTemplateResponseCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentOperator = await _operatorReadRepository.FirstOrDefaultAsync(
                new GetCurrentOperatorForSubmitResponseSpec(currentApplicationUserId),
                cancellationToken);

            if (currentOperator is null)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.CurrentOperatorNotFound",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_CurrentOperator_NotFound,
                    Type: ErrorType.NotFound));
            }

            var assignedTemplate = await _operatorTemplateReadRepository.FirstOrDefaultAsync(
                new GetAssignedTemplateForSubmitResponseSpec(
                    operatorId: currentOperator.OperatorId,
                    templateId: request.TemplateId),
                cancellationToken);

            if (assignedTemplate is null)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.TemplateNotAssigned",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Template_NotAssigned,
                    Type: ErrorType.Security));
            }
            if (!assignedTemplate.TemplateIsActive)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.TemplateInactive",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Template_Inactive,
                    Type: ErrorType.Validation));
            }

            var templateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForSubmitResponseSpec(request.TemplateId),
                cancellationToken);

            if (templateQuestions.Count == 0)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.TemplateHasNoQuestions",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Template_HasNoQuestions,
                    Type: ErrorType.Validation));
            }

            var submittedAnswers = request.Answers.ToArray();

            var duplicateQuestionExists = submittedAnswers
                .GroupBy(x => x.QuestionId)
                .Any(x => x.Count() > 1);

            if (duplicateQuestionExists)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.DuplicateQuestionAnswers",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_DuplicateQuestionAnswers,
                    Type: ErrorType.Validation));
            }

            var templateQuestionIds = templateQuestions
                .Select(x => x.QuestionId)
                .ToHashSet();

            var submittedQuestionIds = submittedAnswers
                .Select(x => x.QuestionId)
                .ToHashSet();

            var hasUnknownQuestion = submittedQuestionIds
                .Any(x => !templateQuestionIds.Contains(x));

            if (hasUnknownQuestion)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.QuestionNotInTemplate",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Question_NotInTemplate,
                    Type: ErrorType.Validation));
            }

            var conditions = await _conditionReadRepository.ListAsync(
     new GetTemplateQuestionConditionsForSubmitResponseSpec(request.TemplateId),
     cancellationToken);

            var validConditions = FilterValidConditions(
                templateQuestions,
                conditions);

            var visibleQuestionIds = CalculateVisibleQuestionIds(
                templateQuestions,
                validConditions,
                submittedAnswers);

            var hasHiddenQuestionAnswer = submittedQuestionIds
                .Any(questionId => !visibleQuestionIds.Contains(questionId));

            if (hasHiddenQuestionAnswer)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.HiddenQuestionAnswerNotAllowed",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_HiddenQuestionAnswer_NotAllowed,
                    Type: ErrorType.Validation));
            }

            var allVisibleQuestionsAnswered = visibleQuestionIds
                .All(questionId => submittedQuestionIds.Contains(questionId));

            if (!allVisibleQuestionsAnswered)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.VisibleQuestionsRequired",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_VisibleQuestions_Required,
                    Type: ErrorType.Validation));
            }

            var questionIds = templateQuestions
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            var options = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForSubmitResponseSpec(questionIds),
                cancellationToken);

            var optionsByQuestionId = options
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.Select(option => option.OptionId).ToHashSet());
            var optionValueByOptionId = options
    .ToDictionary(
        x => x.OptionId,
        x => x.Value);

            var questionsById = templateQuestions
                .ToDictionary(x => x.QuestionId, x => x);

            foreach (var answer in submittedAnswers)
            {
                var question = questionsById[answer.QuestionId];

                var validationError = ValidateAnswerShape(
                    answer,
                    question.Type,
                    optionsByQuestionId);

                if (validationError is not null)
                {
                    return Result<SubmitOperatorTemplateResponseResponse>.Fail(validationError);
                }
            }
            var rootQuestionIds = CalculateRootQuestionIds(
    templateQuestions,
    validConditions);

            var score = CalculateScore(
                submittedAnswers,
                questionsById,
                rootQuestionIds,
                optionValueByOptionId);

            var surveyResponse = SurveyResponse.Create(
      operatorId: currentOperator.OperatorId,
      templateId: request.TemplateId,
      createdByApplicationUserId: currentApplicationUserId,
      actualScore: score.ActualScore,
      maxScore: score.MaxScore,
      scorePercentage: score.Percentage);

            var savedVoiceFileNames = new List<string>();

            try
            {
                foreach (var answer in submittedAnswers)
                {
                    var question = questionsById[answer.QuestionId];

                    var surveyAnswer = await CreateSurveyAnswerAsync(
                        surveyResponse.Id,
                        answer,
                        question.Type,
                        savedVoiceFileNames,
                        cancellationToken);

                    surveyResponse.AddAnswer(surveyAnswer);
                }

                await _surveyResponseWriteRepository.AddAsync(
                    surveyResponse,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                RemoveSavedVoiceFiles(savedVoiceFileNames);
                throw;
            }
            var response = new SubmitOperatorTemplateResponseResponse
            {
                SurveyResponseId = surveyResponse.Id,
                OperatorId = surveyResponse.OperatorId,
                TemplateId = surveyResponse.TemplateId,
                AnswersCount = surveyResponse.Answers.Count,
                ActualScore = surveyResponse.ActualScore,
                MaxScore = surveyResponse.MaxScore,
                ScorePercentage = surveyResponse.ScorePercentage,
                SubmittedOnUtc = surveyResponse.SubmittedOnUtc
            };

            return Result<SubmitOperatorTemplateResponseResponse>.Ok(response);
        }

        private static TemplateQuestionConditionForSubmitResponseDto[] FilterValidConditions(
    IReadOnlyCollection<TemplateQuestionForSubmitResponseDto> templateQuestions,
    IReadOnlyCollection<TemplateQuestionConditionForSubmitResponseDto> conditions)
        {
            if (templateQuestions.Count == 0 || conditions.Count == 0)
            {
                return Array.Empty<TemplateQuestionConditionForSubmitResponseDto>();
            }

            var existingTemplateQuestionIds = templateQuestions
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

        private static HashSet<Guid> CalculateVisibleQuestionIds(
      IReadOnlyCollection<TemplateQuestionForSubmitResponseDto> templateQuestions,
      IReadOnlyCollection<TemplateQuestionConditionForSubmitResponseDto> conditions,
      IReadOnlyCollection<SubmitOperatorTemplateAnswerCommandItem> submittedAnswers)
        {
            var validConditions = FilterValidConditions(
                templateQuestions,
                conditions);

            var questionIdByTemplateQuestionId = templateQuestions
                .ToDictionary(
                    x => x.TemplateQuestionId,
                    x => x.QuestionId);

            var templateQuestionIdByQuestionId = templateQuestions
                .ToDictionary(
                    x => x.QuestionId,
                    x => x.TemplateQuestionId);

            var allTemplateQuestionIds = templateQuestions
                .Select(x => x.TemplateQuestionId)
                .ToHashSet();

            var childTemplateQuestionIds = validConditions
                .Select(x => x.ChildTemplateQuestionId)
                .ToHashSet();

            var rootTemplateQuestionIds = templateQuestions
                .Where(x => !childTemplateQuestionIds.Contains(x.TemplateQuestionId))
                .OrderBy(x => x.Order)
                .Select(x => x.TemplateQuestionId)
                .ToArray();

            if (rootTemplateQuestionIds.Length == 0)
            {
                rootTemplateQuestionIds = templateQuestions
                    .OrderBy(x => x.Order)
                    .Select(x => x.TemplateQuestionId)
                    .ToArray();
            }

            var submittedAnswersByTemplateQuestionId = submittedAnswers
                .Where(answer => templateQuestionIdByQuestionId.ContainsKey(answer.QuestionId))
                .GroupBy(answer => templateQuestionIdByQuestionId[answer.QuestionId])
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

                if (!submittedAnswersByTemplateQuestionId.TryGetValue(
                        parentTemplateQuestionId,
                        out var parentAnswer))
                {
                    continue;
                }

                if (!conditionsByParentTemplateQuestionId.TryGetValue(
                        parentTemplateQuestionId,
                        out var childConditions))
                {
                    continue;
                }

                foreach (var condition in childConditions)
                {
                    if (!allTemplateQuestionIds.Contains(condition.ChildTemplateQuestionId))
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

            return visibleTemplateQuestionIds
                .Where(questionIdByTemplateQuestionId.ContainsKey)
                .Select(templateQuestionId => questionIdByTemplateQuestionId[templateQuestionId])
                .ToHashSet();
        }

        private static bool ConditionMatchesAnswer(
            TemplateQuestionConditionForSubmitResponseDto condition,
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return condition.TriggerType switch
            {
                QuestionConditionTriggerType.SingleChoiceOption =>
                    answer.SelectedQuestionOptionId.HasValue &&
                    condition.SelectedQuestionOptionId.HasValue &&
                    answer.SelectedQuestionOptionId.Value == condition.SelectedQuestionOptionId.Value,

                QuestionConditionTriggerType.StarRatingValue =>
                    answer.StarRatingValue.HasValue &&
                    condition.TriggerValue.HasValue &&
                    answer.StarRatingValue.Value == condition.TriggerValue.Value,

                QuestionConditionTriggerType.SmileValue =>
                    answer.SmileValue.HasValue &&
                    condition.TriggerValue.HasValue &&
                    answer.SmileValue.Value == condition.TriggerValue.Value,

                _ => false
            };
        }

        private async Task<SurveyAnswer> CreateSurveyAnswerAsync(
            Guid surveyResponseId,
            SubmitOperatorTemplateAnswerCommandItem answer,
            QuestionType questionType,
            List<string> savedVoiceFileNames,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return questionType switch
            {
                QuestionType.SingleChoice => SurveyAnswer.CreateSingleChoice(
                    surveyResponseId,
                    answer.QuestionId,
                    answer.SelectedQuestionOptionId!.Value),

                QuestionType.StarRating => SurveyAnswer.CreateStarRating(
                    surveyResponseId,
                    answer.QuestionId,
                    answer.StarRatingValue!.Value),

                QuestionType.Smiles => SurveyAnswer.CreateSmiles(
                    surveyResponseId,
                    answer.QuestionId,
                    answer.SmileValue!.Value),

                QuestionType.Complain => SurveyAnswer.CreateComplain(
                    surveyResponseId,
                    answer.QuestionId,
                    answer.TextAnswer!),

                QuestionType.Voice => await CreateVoiceAnswerAsync(
                    surveyResponseId,
                    answer,
                    savedVoiceFileNames),

                _ => throw new InvalidOperationException(
                    $"Unsupported question type '{questionType}'.")
            };
        }

        private async Task<SurveyAnswer> CreateVoiceAnswerAsync(
            Guid surveyResponseId,
            SubmitOperatorTemplateAnswerCommandItem answer,
            List<string> savedVoiceFileNames)
        {
            var fileName = await _mediaService.SaveAsync(
                answer.VoiceFile!,
                FileNames.SurveyVoiceAnswers);

            savedVoiceFileNames.Add(fileName);

            return SurveyAnswer.CreateVoice(
                surveyResponseId,
                answer.QuestionId,
                fileName);
        }

        private static Error? ValidateAnswerShape(
            SubmitOperatorTemplateAnswerCommandItem answer,
            QuestionType questionType,
            IReadOnlyDictionary<Guid, HashSet<Guid>> optionsByQuestionId)
        {
            return questionType switch
            {
                QuestionType.SingleChoice => ValidateSingleChoice(
                    answer,
                    optionsByQuestionId),

                QuestionType.Voice => ValidateVoice(answer),

                QuestionType.StarRating => ValidateStarRating(answer),

                QuestionType.Complain => ValidateComplain(answer),

                QuestionType.Smiles => ValidateSmiles(answer),

                _ => new Error(
                    Code: "SurveyResponses.Submit.QuestionTypeUnsupported",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_QuestionType_Unsupported,
                    Type: ErrorType.Validation)
            };
        }

        private static Error? ValidateSingleChoice(
            SubmitOperatorTemplateAnswerCommandItem answer,
            IReadOnlyDictionary<Guid, HashSet<Guid>> optionsByQuestionId)
        {
            if (!OnlySingleChoiceFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SingleChoiceInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_SingleChoice_InvalidShape,
                    Type: ErrorType.Validation);
            }

            if (!optionsByQuestionId.TryGetValue(answer.QuestionId, out var validOptionIds) ||
                !validOptionIds.Contains(answer.SelectedQuestionOptionId!.Value))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SingleChoiceOptionInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_SingleChoice_OptionInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateVoice(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlyVoiceFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.VoiceInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Voice_InvalidShape,
                    Type: ErrorType.Validation);
            }

            if (answer.VoiceFile is null || answer.VoiceFile.Length == 0)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.VoiceFileRequired",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Voice_FileRequired,
                    Type: ErrorType.Validation);
            }

            if (answer.VoiceFile.Length > MaxVoiceFileSizeInBytes)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.VoiceFileTooLarge",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Voice_FileTooLarge,
                    Type: ErrorType.Validation);
            }

            var extension = Path.GetExtension(answer.VoiceFile.FileName);

            var extensionAllowed = AllowedVoiceExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase);

            if (!extensionAllowed)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.VoiceFileExtensionInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Voice_FileExtensionInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateStarRating(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlyStarRatingFieldsProvided(answer) ||
                answer.StarRatingValue!.Value < 1 ||
                answer.StarRatingValue.Value > 5)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.StarRatingValueInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_StarRating_ValueInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateComplain(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlyComplainFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.ComplainInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Complain_InvalidShape,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateSmiles(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlySmilesFieldsProvided(answer) ||
                answer.SmileValue!.Value < 1 ||
                answer.SmileValue.Value > 5)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SmilesValueInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Smiles_ValueInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static bool OnlySingleChoiceFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return answer.SelectedQuestionOptionId.HasValue &&
                   !answer.StarRatingValue.HasValue &&
                   !answer.SmileValue.HasValue &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlyVoiceFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return !answer.SelectedQuestionOptionId.HasValue &&
                   !answer.StarRatingValue.HasValue &&
                   !answer.SmileValue.HasValue &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is not null;
        }

        private static bool OnlyStarRatingFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return !answer.SelectedQuestionOptionId.HasValue &&
                   answer.StarRatingValue.HasValue &&
                   !answer.SmileValue.HasValue &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlyComplainFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return !answer.SelectedQuestionOptionId.HasValue &&
                   !answer.StarRatingValue.HasValue &&
                   !answer.SmileValue.HasValue &&
                   !string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlySmilesFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return !answer.SelectedQuestionOptionId.HasValue &&
                   !answer.StarRatingValue.HasValue &&
                   answer.SmileValue.HasValue &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private void RemoveSavedVoiceFiles(IEnumerable<string> savedVoiceFileNames)
        {
            var filePaths = savedVoiceFileNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(fileName => Path.Combine(
                    "./wwwroot/Media",
                    FileNames.SurveyVoiceAnswers,
                    fileName));

            _mediaService.RemoveRange(filePaths);
        }

        private static HashSet<Guid> CalculateRootQuestionIds(
    IReadOnlyCollection<TemplateQuestionForSubmitResponseDto> templateQuestions,
    IReadOnlyCollection<TemplateQuestionConditionForSubmitResponseDto> conditions)
        {
            var validConditions = FilterValidConditions(
                templateQuestions,
                conditions);

            var childTemplateQuestionIds = validConditions
                .Select(x => x.ChildTemplateQuestionId)
                .ToHashSet();

            var rootQuestionIds = templateQuestions
                .Where(x => !childTemplateQuestionIds.Contains(x.TemplateQuestionId))
                .OrderBy(x => x.Order)
                .Select(x => x.QuestionId)
                .ToHashSet();

            if (rootQuestionIds.Count > 0)
            {
                return rootQuestionIds;
            }

            // Safety fallback if bad data caused every question to be child.
            return templateQuestions
                .OrderBy(x => x.Order)
                .Select(x => x.QuestionId)
                .ToHashSet();
        }

        private static SubmitTemplateScoreResult CalculateScore(
            IReadOnlyCollection<SubmitOperatorTemplateAnswerCommandItem> submittedAnswers,
            IReadOnlyDictionary<Guid, TemplateQuestionForSubmitResponseDto> questionsById,
            IReadOnlySet<Guid> rootQuestionIds,
            IReadOnlyDictionary<Guid, int> optionValueByOptionId)
        {
            var actualScore = 0;
            var maxScore = 0;

            foreach (var answer in submittedAnswers)
            {
                if (!rootQuestionIds.Contains(answer.QuestionId))
                {
                    continue;
                }

                if (!questionsById.TryGetValue(answer.QuestionId, out var question))
                {
                    continue;
                }

                switch (question.Type)
                {
                    case QuestionType.SingleChoice:
                        if (answer.SelectedQuestionOptionId.HasValue &&
                            optionValueByOptionId.TryGetValue(answer.SelectedQuestionOptionId.Value, out var optionValue))
                        {
                            actualScore += optionValue;
                            maxScore += 5;
                        }

                        break;

                    case QuestionType.StarRating:
                        if (answer.StarRatingValue.HasValue)
                        {
                            actualScore += answer.StarRatingValue.Value;
                            maxScore += 5;
                        }

                        break;

                    case QuestionType.Smiles:
                        if (answer.SmileValue.HasValue)
                        {
                            actualScore += answer.SmileValue.Value;
                            maxScore += 5;
                        }

                        break;

                    case QuestionType.Voice:
                    case QuestionType.Complain:
                    default:
                        break;
                }
            }

            var percentage = maxScore == 0
                ? 0m
                : Math.Round((decimal)actualScore / maxScore * 100m, 2, MidpointRounding.AwayFromZero);

            return new SubmitTemplateScoreResult(
                ActualScore: actualScore,
                MaxScore: maxScore,
                Percentage: percentage);
        }

        private sealed record SubmitTemplateScoreResult(
            int ActualScore,
            int MaxScore,
            decimal Percentage);
    }
}