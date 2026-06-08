using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Shared.Validation;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed class SubmitAnonymousTemplateResponseCommandHandler
        : ICommandHandler<SubmitAnonymousTemplateResponseCommand, SubmitAnonymousTemplateResponseResult>
    {
        private readonly IWriteReadRepository<AnonymousTemplate> _anonymousTemplateReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateCustomInput> _customInputReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestion> _anonymousTemplateQuestionReadRepository;
        private readonly IWriteReadRepository<AnonymousTemplateQuestionCondition> _conditionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteRepository<AnonymousSurveyResponse> _anonymousSurveyResponseWriteRepository;
        private readonly IMediaService _mediaService;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitAnonymousTemplateResponseCommandHandler(
            IWriteReadRepository<AnonymousTemplate> anonymousTemplateReadRepository,
            IWriteReadRepository<AnonymousTemplateCustomInput> customInputReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestion> anonymousTemplateQuestionReadRepository,
            IWriteReadRepository<AnonymousTemplateQuestionCondition> conditionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteRepository<AnonymousSurveyResponse> anonymousSurveyResponseWriteRepository,
            IMediaService mediaService,
            IUnitOfWork unitOfWork)
        {
            _anonymousTemplateReadRepository = anonymousTemplateReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateReadRepository));

            _customInputReadRepository = customInputReadRepository
                ?? throw new ArgumentNullException(nameof(customInputReadRepository));

            _anonymousTemplateQuestionReadRepository = anonymousTemplateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(anonymousTemplateQuestionReadRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _anonymousSurveyResponseWriteRepository = anonymousSurveyResponseWriteRepository
                ?? throw new ArgumentNullException(nameof(anonymousSurveyResponseWriteRepository));

            _mediaService = mediaService
                ?? throw new ArgumentNullException(nameof(mediaService));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<SubmitAnonymousTemplateResponseResult>> Handle(
            SubmitAnonymousTemplateResponseCommand request,
            CancellationToken cancellationToken)
        {
            var anonymousTemplate = await _anonymousTemplateReadRepository.FirstOrDefaultAsync(
                new GetAnonymousTemplateForSubmitResponseSpec(request.AnonymousTemplateId),
                cancellationToken);

            if (anonymousTemplate is null)
            {
                return Result<SubmitAnonymousTemplateResponseResult>.Fail(new Error(
                    Code: "AnonTemplates.Submit.TemplateNotFound",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var availabilityResult = ValidateTemplateAvailability(anonymousTemplate);

            if (availabilityResult.Error is not null)
            {
                return Result<SubmitAnonymousTemplateResponseResult>.Fail(
                    availabilityResult.Error);
            }

            var customInputs = await _customInputReadRepository.ListAsync(
                new GetAnonymousTemplateCustomInputsForSubmitSpec(anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            var customInputValidationResult = ValidateCustomInputValues(
                customInputs,
                request.CustomInputValues);

            if (customInputValidationResult.Error is not null)
            {
                return Result<SubmitAnonymousTemplateResponseResult>.Fail(
                    customInputValidationResult.Error);
            }

            var templateQuestions = await _anonymousTemplateQuestionReadRepository.ListAsync(
                new GetAnonymousTemplateQuestionsForSubmitSpec(anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            var activeTemplateQuestions = templateQuestions
                .Where(x => x.QuestionIsActive && x.GroupIsActive)
                .OrderBy(x => x.Order)
                .ToArray();

            if (activeTemplateQuestions.Length == 0)
            {
                return Result<SubmitAnonymousTemplateResponseResult>.Fail(new Error(
                    Code: "AnonTemplates.Submit.TemplateHasNoQuestions",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_Template_HasNoQuestions,
                    Type: ErrorType.Validation));
            }

            var allConditions = await _conditionReadRepository.ListAsync(
                new GetAnonymousTemplateConditionsForSubmitSpec(anonymousTemplate.AnonymousTemplateId),
                cancellationToken);

            var validConditions = FilterValidConditions(
                allConditions,
                activeTemplateQuestions);

            var singleChoiceQuestionIds = activeTemplateQuestions
                .Where(x => x.QuestionType == QuestionType.SingleChoice)
                .Select(x => x.QuestionId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<QuestionOptionForSubmitDto> questionOptions;

            if (singleChoiceQuestionIds.Length == 0)
            {
                questionOptions = Array.Empty<QuestionOptionForSubmitDto>();
            }
            else
            {
                questionOptions = await _questionOptionReadRepository.ListAsync(
                    new GetQuestionOptionsForSubmitResponseSpec(singleChoiceQuestionIds),
                    cancellationToken);
            }

            var answerValidationResult = ValidateAnswers(
                request.Answers,
                activeTemplateQuestions,
                validConditions,
                questionOptions);

            if (answerValidationResult.Error is not null)
            {
                return Result<SubmitAnonymousTemplateResponseResult>.Fail(
                    answerValidationResult.Error);
            }

            var scoringResult = CalculateScore(
                answerValidationResult.VisibleQuestions,
                answerValidationResult.RootQuestionIds,
                request.Answers,
                questionOptions);

            var anonymousSurveyResponse = AnonymousSurveyResponse.Create(
                anonymousTemplateId: anonymousTemplate.AnonymousTemplateId,
                actualScore: scoringResult.ActualScore,
                maxScore: scoringResult.MaxScore,
                scorePercentage: scoringResult.ScorePercentage);

            AddCustomInputValuesToResponse(
                anonymousSurveyResponse,
                customInputs,
                request.CustomInputValues);

            var savedImageFileNames = new List<string>();

            try
            {
                await AddAnswersToResponseAsync(
                    anonymousSurveyResponse,
                    activeTemplateQuestions,
                    request.Answers,
                    savedImageFileNames,
                    cancellationToken);

                await _anonymousSurveyResponseWriteRepository.AddAsync(
                    anonymousSurveyResponse,
                    cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                RemoveSavedImageFiles(savedImageFileNames);
                throw;
            }

            return Result<SubmitAnonymousTemplateResponseResult>.Ok(
                new SubmitAnonymousTemplateResponseResult
                {
                    AnonymousSurveyResponseId = anonymousSurveyResponse.Id,
                    AnonymousTemplateId = anonymousTemplate.AnonymousTemplateId,
                    SubmittedOnUtc = anonymousSurveyResponse.SubmittedOnUtc,
                    ActualScore = anonymousSurveyResponse.ActualScore,
                    MaxScore = anonymousSurveyResponse.MaxScore,
                    ScorePercentage = anonymousSurveyResponse.ScorePercentage,
                    VisibleQuestionsCount = answerValidationResult.VisibleQuestions.Count,
                    AnswersCount = anonymousSurveyResponse.Answers.Count,
                    CustomInputValuesCount = anonymousSurveyResponse.CustomInputValues.Count
                });
        }

        private static ValidationResult ValidateTemplateAvailability(
            AnonymousTemplateForSubmitResponseDto anonymousTemplate)
        {
            var utcNow = DateTime.UtcNow;

            if (!anonymousTemplate.IsActive)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.TemplateNotAvailable",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_Template_NotAvailable,
                    Type: ErrorType.NotFound));
            }

            if (anonymousTemplate.ActiveFrom > utcNow)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.TemplateNotStarted",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_Template_NotStarted,
                    Type: ErrorType.Validation));
            }

            if (anonymousTemplate.ExpireTo.HasValue &&
                anonymousTemplate.ExpireTo.Value <= utcNow)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.TemplateExpired",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_Template_Expired,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateCustomInputValues(
            IReadOnlyCollection<AnonymousTemplateCustomInputForSubmitDto> customInputs,
            IReadOnlyCollection<SubmitAnonymousTemplateCustomInputValueCommandItem> submittedValues)
        {
            submittedValues ??= Array.Empty<SubmitAnonymousTemplateCustomInputValueCommandItem>();

            var customInputsById = customInputs.ToDictionary(x => x.CustomInputId);

            foreach (var submittedValue in submittedValues)
            {
                if (!customInputsById.ContainsKey(submittedValue.CustomInputId))
                {
                    return ValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.CustomInputNotFound",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_NotFound,
                        Type: ErrorType.Validation));
                }
            }

            var submittedValuesByInputId = submittedValues.ToDictionary(x => x.CustomInputId);

            foreach (var customInput in customInputs)
            {
                submittedValuesByInputId.TryGetValue(
                    customInput.CustomInputId,
                    out var submittedValue);

                if (customInput.IsRequired && submittedValue is null)
                {
                    return ValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.RequiredCustomInputMissing",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_Required,
                        Type: ErrorType.Validation));
                }

                if (submittedValue is null)
                {
                    continue;
                }

                var result = ValidateSingleCustomInputValue(
                    customInput,
                    submittedValue);

                if (result.Error is not null)
                {
                    return result;
                }
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateSingleCustomInputValue(
            AnonymousTemplateCustomInputForSubmitDto customInput,
            SubmitAnonymousTemplateCustomInputValueCommandItem submittedValue)
        {
            return customInput.Type switch
            {
                TemplateCustomInputType.String =>
                    ValidateStringCustomInputValue(customInput, submittedValue),

                TemplateCustomInputType.Integer =>
                    ValidateIntegerCustomInputValue(customInput, submittedValue),

                _ => ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputTypeInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_Type_Invalid,
                    Type: ErrorType.Validation))
            };
        }

        private static ValidationResult ValidateStringCustomInputValue(
            AnonymousTemplateCustomInputForSubmitDto customInput,
            SubmitAnonymousTemplateCustomInputValueCommandItem submittedValue)
        {
            if (submittedValue.IntegerValue.HasValue)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputStringShapeInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_StringShape_Invalid,
                    Type: ErrorType.Validation));
            }

            var value = submittedValue.StringValue?.Trim();

            if (customInput.IsRequired && string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputRequired",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_Required,
                    Type: ErrorType.Validation));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResult.Ok();
            }

            if (customInput.MinLength.HasValue &&
                value.Length < customInput.MinLength.Value)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputMinLength",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_MinLength_Invalid,
                    Type: ErrorType.Validation));
            }

            if (customInput.MaxLength.HasValue &&
                value.Length > customInput.MaxLength.Value)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputMaxLength",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_MaxLength_Invalid,
                    Type: ErrorType.Validation));
            }

            if (!string.IsNullOrEmpty(customInput.StartWith) &&
                !value.StartsWith(customInput.StartWith, StringComparison.Ordinal))
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputStartWithInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_StartWith_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateIntegerCustomInputValue(
            AnonymousTemplateCustomInputForSubmitDto customInput,
            SubmitAnonymousTemplateCustomInputValueCommandItem submittedValue)
        {
            if (!string.IsNullOrWhiteSpace(submittedValue.StringValue))
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputIntegerShapeInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_IntegerShape_Invalid,
                    Type: ErrorType.Validation));
            }

            if (customInput.IsRequired && !submittedValue.IntegerValue.HasValue)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputRequired",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_Required,
                    Type: ErrorType.Validation));
            }

            if (!submittedValue.IntegerValue.HasValue)
            {
                return ValidationResult.Ok();
            }

            if (customInput.MinValue.HasValue &&
                submittedValue.IntegerValue.Value < customInput.MinValue.Value)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputMinValue",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_MinValue_Invalid,
                    Type: ErrorType.Validation));
            }

            if (customInput.MaxValue.HasValue &&
                submittedValue.IntegerValue.Value > customInput.MaxValue.Value)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.CustomInputMaxValue",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_CustomInput_MaxValue_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static IReadOnlyCollection<AnonymousTemplateConditionForSubmitDto> FilterValidConditions(
            IReadOnlyCollection<AnonymousTemplateConditionForSubmitDto> conditions,
            IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> templateQuestions)
        {
            var templateQuestionIds = templateQuestions
                .Select(x => x.AnonymousTemplateQuestionId)
                .ToHashSet();

            return conditions
                .Where(x =>
                    x.IsActive &&
                    templateQuestionIds.Contains(x.ParentAnonymousTemplateQuestionId) &&
                    templateQuestionIds.Contains(x.ChildAnonymousTemplateQuestionId))
                .OrderBy(x => x.Order)
                .ToArray();
        }

        private static AnswerValidationResult ValidateAnswers(
            IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> submittedAnswers,
            IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> templateQuestions,
            IReadOnlyCollection<AnonymousTemplateConditionForSubmitDto> conditions,
            IReadOnlyCollection<QuestionOptionForSubmitDto> questionOptions)
        {
            submittedAnswers ??= Array.Empty<SubmitAnonymousTemplateAnswerCommandItem>();

            var templateQuestionsById = templateQuestions
                .ToDictionary(x => x.AnonymousTemplateQuestionId);

            foreach (var submittedAnswer in submittedAnswers)
            {
                if (!templateQuestionsById.ContainsKey(submittedAnswer.AnonymousTemplateQuestionId))
                {
                    return AnswerValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.AnswerQuestionNotFound",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_Question_NotFound,
                        Type: ErrorType.Validation));
                }
            }

            var submittedAnswersByTemplateQuestionId = submittedAnswers
                .ToDictionary(x => x.AnonymousTemplateQuestionId);

            var optionValidationResult = ValidateAnswerShapesAndOptions(
                submittedAnswers,
                templateQuestionsById,
                questionOptions);

            if (optionValidationResult.Error is not null)
            {
                return AnswerValidationResult.Fail(optionValidationResult.Error);
            }

            var rootQuestionIds = GetRootQuestionIds(
                templateQuestions,
                conditions);

            var visibleQuestionIds = ResolveVisibleQuestionIds(
                rootQuestionIds,
                conditions,
                submittedAnswersByTemplateQuestionId);

            foreach (var visibleQuestionId in visibleQuestionIds)
            {
                if (!submittedAnswersByTemplateQuestionId.ContainsKey(visibleQuestionId))
                {
                    return AnswerValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.VisibleQuestionMissingAnswer",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_AllVisibleQuestions_Required,
                        Type: ErrorType.Validation));
                }
            }

            foreach (var submittedAnswer in submittedAnswers)
            {
                if (!visibleQuestionIds.Contains(submittedAnswer.AnonymousTemplateQuestionId))
                {
                    return AnswerValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.HiddenQuestionAnswerNotAllowed",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_HiddenQuestion_NotAllowed,
                        Type: ErrorType.Validation));
                }
            }

            var visibleQuestions = templateQuestions
                .Where(x => visibleQuestionIds.Contains(x.AnonymousTemplateQuestionId))
                .OrderBy(x => x.Order)
                .ToArray();

            return AnswerValidationResult.Ok(
                visibleQuestions,
                rootQuestionIds);
        }

        private static ValidationResult ValidateAnswerShapesAndOptions(
            IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> submittedAnswers,
            IReadOnlyDictionary<Guid, AnonymousTemplateQuestionForSubmitDto> templateQuestionsById,
            IReadOnlyCollection<QuestionOptionForSubmitDto> questionOptions)
        {
            var optionsById = questionOptions
                .ToDictionary(x => x.OptionId);

            foreach (var answer in submittedAnswers)
            {
                var question = templateQuestionsById[answer.AnonymousTemplateQuestionId];

                var validationResult = question.QuestionType switch
                {
                    QuestionType.SingleChoice =>
                        ValidateSingleChoiceAnswer(answer, question, optionsById),

                    QuestionType.StarRating =>
                        ValidateStarRatingAnswer(answer),

                    QuestionType.Smiles =>
                        ValidateSmilesAnswer(answer),

                    QuestionType.Complain =>
                        ValidateComplainAnswer(answer),

                    QuestionType.Voice =>
                        ValidateVoiceAnswer(answer),

                    QuestionType.Image =>
                        ValidateImageAnswer(answer),

                    _ => ValidationResult.Fail(new Error(
                        Code: "AnonTemplates.Submit.QuestionTypeUnsupported",
                        Message: ErrorMessage.SubmitAnonymousTemplateResponse_QuestionType_Unsupported,
                        Type: ErrorType.Validation))
                };

                if (validationResult.Error is not null)
                {
                    return validationResult;
                }
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateSingleChoiceAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer,
            AnonymousTemplateQuestionForSubmitDto question,
            IReadOnlyDictionary<Guid, QuestionOptionForSubmitDto> optionsById)
        {
            if (!answer.SelectedQuestionOptionId.HasValue ||
                answer.SelectedQuestionOptionId.Value == Guid.Empty ||
                answer.StarRatingValue.HasValue ||
                answer.SmileValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                !string.IsNullOrWhiteSpace(answer.VoiceFileName) ||
                answer.ImageFile is not null)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.SingleChoiceAnswerInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_SingleChoiceAnswer_Invalid,
                    Type: ErrorType.Validation));
            }

            if (!optionsById.TryGetValue(
                    answer.SelectedQuestionOptionId.Value,
                    out var selectedOption))
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.SelectedOptionNotFound",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_SelectedOption_NotFound,
                    Type: ErrorType.Validation));
            }

            if (!selectedOption.IsActive)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.SelectedOptionInactive",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_SelectedOption_Inactive,
                    Type: ErrorType.Validation));
            }

            if (selectedOption.QuestionId != question.QuestionId)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.SelectedOptionNotBelongToQuestion",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_SelectedOption_NotBelongToQuestion,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateStarRatingAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer)
        {
            if (answer.StarRatingValue is not >= 1 and <= 5 ||
                answer.SelectedQuestionOptionId.HasValue ||
                answer.SmileValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                !string.IsNullOrWhiteSpace(answer.VoiceFileName) ||
                answer.ImageFile is not null)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.StarRatingAnswerInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_StarRatingAnswer_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateSmilesAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer)
        {
            if (answer.SmileValue is not >= 1 and <= 5 ||
                answer.SelectedQuestionOptionId.HasValue ||
                answer.StarRatingValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                !string.IsNullOrWhiteSpace(answer.VoiceFileName) ||
                answer.ImageFile is not null)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.SmilesAnswerInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_SmilesAnswer_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateComplainAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer)
        {
            if (string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                answer.SelectedQuestionOptionId.HasValue ||
                answer.StarRatingValue.HasValue ||
                answer.SmileValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.VoiceFileName) ||
                answer.ImageFile is not null)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.ComplainAnswerInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_ComplainAnswer_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateVoiceAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer)
        {
            if (string.IsNullOrWhiteSpace(answer.VoiceFileName) ||
                answer.SelectedQuestionOptionId.HasValue ||
                answer.StarRatingValue.HasValue ||
                answer.SmileValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                answer.ImageFile is not null)
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.VoiceAnswerInvalid",
                    Message: ErrorMessage.SubmitAnonymousTemplateResponse_VoiceAnswer_Invalid,
                    Type: ErrorType.Validation));
            }

            return ValidationResult.Ok();
        }

        private static ValidationResult ValidateImageAnswer(
            SubmitAnonymousTemplateAnswerCommandItem answer)
        {
            if (answer.SelectedQuestionOptionId.HasValue ||
                answer.StarRatingValue.HasValue ||
                answer.SmileValue.HasValue ||
                !string.IsNullOrWhiteSpace(answer.TextAnswer) ||
                !string.IsNullOrWhiteSpace(answer.VoiceFileName))
            {
                return ValidationResult.Fail(new Error(
                    Code: "AnonTemplates.Submit.ImageAnswerInvalidShape",
                    Message: ErrorMessage.SubmitResponse_ImageAnswer_InvalidShape,
                    Type: ErrorType.Validation));
            }

            var fileValidationError = ImageAnswerFileValidator.Validate(answer.ImageFile);

            return fileValidationError is null
                ? ValidationResult.Ok()
                : ValidationResult.Fail(fileValidationError);
        }

        private static IReadOnlyCollection<Guid> GetRootQuestionIds(
            IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> templateQuestions,
            IReadOnlyCollection<AnonymousTemplateConditionForSubmitDto> conditions)
        {
            var childQuestionIds = conditions
                .Select(x => x.ChildAnonymousTemplateQuestionId)
                .Distinct()
                .ToHashSet();

            return templateQuestions
                .Where(x => !childQuestionIds.Contains(x.AnonymousTemplateQuestionId))
                .OrderBy(x => x.Order)
                .Select(x => x.AnonymousTemplateQuestionId)
                .ToArray();
        }

        private static HashSet<Guid> ResolveVisibleQuestionIds(
            IReadOnlyCollection<Guid> rootQuestionIds,
            IReadOnlyCollection<AnonymousTemplateConditionForSubmitDto> conditions,
            IReadOnlyDictionary<Guid, SubmitAnonymousTemplateAnswerCommandItem> submittedAnswersByQuestionId)
        {
            var visibleQuestionIds = rootQuestionIds.ToHashSet();

            var conditionsByParentId = conditions
                .GroupBy(x => x.ParentAnonymousTemplateQuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderBy(condition => condition.Order).ToArray());

            var queue = new Queue<Guid>(rootQuestionIds);

            while (queue.Count > 0)
            {
                var parentQuestionId = queue.Dequeue();

                if (!submittedAnswersByQuestionId.TryGetValue(
                        parentQuestionId,
                        out var parentAnswer))
                {
                    continue;
                }

                if (!conditionsByParentId.TryGetValue(
                        parentQuestionId,
                        out var parentConditions))
                {
                    continue;
                }

                foreach (var condition in parentConditions)
                {
                    if (!IsConditionSatisfied(condition, parentAnswer))
                    {
                        continue;
                    }

                    if (visibleQuestionIds.Add(condition.ChildAnonymousTemplateQuestionId))
                    {
                        queue.Enqueue(condition.ChildAnonymousTemplateQuestionId);
                    }
                }
            }

            return visibleQuestionIds;
        }

        private static bool IsConditionSatisfied(
            AnonymousTemplateConditionForSubmitDto condition,
            SubmitAnonymousTemplateAnswerCommandItem answer)
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

        private static ScoreCalculationResult CalculateScore(
            IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> visibleQuestions,
            IReadOnlyCollection<Guid> rootQuestionIds,
            IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> submittedAnswers,
            IReadOnlyCollection<QuestionOptionForSubmitDto> questionOptions)
        {
            var visibleRootQuestions = visibleQuestions
                .Where(x => rootQuestionIds.Contains(x.AnonymousTemplateQuestionId))
                .ToArray();

            var answersByQuestionId = submittedAnswers
                .ToDictionary(x => x.AnonymousTemplateQuestionId);

            var optionsById = questionOptions
                .Where(x => x.IsActive)
                .ToDictionary(x => x.OptionId);

            var optionsByQuestionId = questionOptions
                .Where(x => x.IsActive)
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToArray());

            var actualScore = 0;
            var maxScore = 0;

            foreach (var question in visibleRootQuestions)
            {
                if (!answersByQuestionId.TryGetValue(
                        question.AnonymousTemplateQuestionId,
                        out var answer))
                {
                    continue;
                }

                switch (question.QuestionType)
                {
                    case QuestionType.SingleChoice:
                        {
                            if (answer.SelectedQuestionOptionId.HasValue &&
                                optionsById.TryGetValue(
                                    answer.SelectedQuestionOptionId.Value,
                                    out var selectedOption))
                            {
                                actualScore += selectedOption.Value;
                            }

                            if (optionsByQuestionId.TryGetValue(question.QuestionId, out var options))
                            {
                                maxScore += options.Max(x => x.Value);
                            }

                            break;
                        }

                    case QuestionType.StarRating:
                        {
                            actualScore += answer.StarRatingValue ?? 0;
                            maxScore += 5;
                            break;
                        }

                    case QuestionType.Smiles:
                        {
                            actualScore += answer.SmileValue ?? 0;
                            maxScore += 5;
                            break;
                        }
                }
            }

            var percentage = maxScore == 0
                ? 0
                : Math.Round((decimal)actualScore / maxScore * 100, 2);

            return new ScoreCalculationResult(
                ActualScore: actualScore,
                MaxScore: maxScore,
                ScorePercentage: percentage);
        }

        private static void AddCustomInputValuesToResponse(
            AnonymousSurveyResponse response,
            IReadOnlyCollection<AnonymousTemplateCustomInputForSubmitDto> customInputs,
            IReadOnlyCollection<SubmitAnonymousTemplateCustomInputValueCommandItem> submittedValues)
        {
            var customInputsById = customInputs.ToDictionary(x => x.CustomInputId);

            foreach (var submittedValue in submittedValues)
            {
                if (!customInputsById.TryGetValue(
                        submittedValue.CustomInputId,
                        out var customInput))
                {
                    continue;
                }

                if (customInput.Type == TemplateCustomInputType.String &&
                    !string.IsNullOrWhiteSpace(submittedValue.StringValue))
                {
                    response.AddCustomInputValue(
                        AnonymousSurveyResponseCustomInputValue.CreateStringValue(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateCustomInputId: customInput.CustomInputId,
                            nameSnapshot: customInput.Name,
                            value: submittedValue.StringValue));
                }

                if (customInput.Type == TemplateCustomInputType.Integer &&
                    submittedValue.IntegerValue.HasValue)
                {
                    response.AddCustomInputValue(
                        AnonymousSurveyResponseCustomInputValue.CreateIntegerValue(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateCustomInputId: customInput.CustomInputId,
                            nameSnapshot: customInput.Name,
                            value: submittedValue.IntegerValue.Value));
                }
            }
        }

        private async Task AddAnswersToResponseAsync(
            AnonymousSurveyResponse response,
            IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> templateQuestions,
            IReadOnlyCollection<SubmitAnonymousTemplateAnswerCommandItem> submittedAnswers,
            List<string> savedImageFileNames,
            CancellationToken cancellationToken)
        {
            var questionsById = templateQuestions.ToDictionary(x => x.AnonymousTemplateQuestionId);

            foreach (var submittedAnswer in submittedAnswers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var question = questionsById[submittedAnswer.AnonymousTemplateQuestionId];

                var answer = question.QuestionType switch
                {
                    QuestionType.SingleChoice =>
                        AnonymousSurveyAnswer.CreateSingleChoice(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateQuestionId: question.AnonymousTemplateQuestionId,
                            questionId: question.QuestionId,
                            selectedQuestionOptionId: submittedAnswer.SelectedQuestionOptionId!.Value),

                    QuestionType.StarRating =>
                        AnonymousSurveyAnswer.CreateStarRating(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateQuestionId: question.AnonymousTemplateQuestionId,
                            questionId: question.QuestionId,
                            value: submittedAnswer.StarRatingValue!.Value),

                    QuestionType.Smiles =>
                        AnonymousSurveyAnswer.CreateSmiles(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateQuestionId: question.AnonymousTemplateQuestionId,
                            questionId: question.QuestionId,
                            value: submittedAnswer.SmileValue!.Value),

                    QuestionType.Complain =>
                        AnonymousSurveyAnswer.CreateComplain(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateQuestionId: question.AnonymousTemplateQuestionId,
                            questionId: question.QuestionId,
                            textAnswer: submittedAnswer.TextAnswer!),

                    QuestionType.Voice =>
                        AnonymousSurveyAnswer.CreateVoice(
                            anonymousSurveyResponseId: response.Id,
                            anonymousTemplateQuestionId: question.AnonymousTemplateQuestionId,
                            questionId: question.QuestionId,
                            voiceFileName: submittedAnswer.VoiceFileName!),

                    QuestionType.Image =>
                        await CreateImageAnswerAsync(
                            response.Id,
                            question,
                            submittedAnswer,
                            savedImageFileNames),

                    _ => throw new InvalidOperationException("Unsupported anonymous answer type.")
                };

                response.AddAnswer(answer);
            }
        }

        private async Task<AnonymousSurveyAnswer> CreateImageAnswerAsync(
            Guid anonymousSurveyResponseId,
            AnonymousTemplateQuestionForSubmitDto question,
            SubmitAnonymousTemplateAnswerCommandItem submittedAnswer,
            List<string> savedImageFileNames)
        {
            var fileName = await _mediaService.SaveAsync(
                submittedAnswer.ImageFile!,
                FileNames.SurveyAnswerImages);

            savedImageFileNames.Add(fileName);

            return AnonymousSurveyAnswer.CreateImage(
                anonymousSurveyResponseId,
                question.AnonymousTemplateQuestionId,
                question.QuestionId,
                fileName);
        }

        private void RemoveSavedImageFiles(IEnumerable<string> savedImageFileNames)
        {
            var filePaths = savedImageFileNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(fileName => Path.Combine(
                    "./wwwroot/Media",
                    FileNames.SurveyAnswerImages,
                    fileName));

            _mediaService.RemoveRange(filePaths);
        }

        private sealed class ValidationResult
        {
            private ValidationResult(Error? error)
            {
                Error = error;
            }

            public Error? Error { get; }

            public static ValidationResult Ok()
            {
                return new ValidationResult(error: null);
            }

            public static ValidationResult Fail(Error error)
            {
                return new ValidationResult(error);
            }
        }

        private sealed class AnswerValidationResult
        {
            private AnswerValidationResult(
                Error? error,
                IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> visibleQuestions,
                IReadOnlyCollection<Guid> rootQuestionIds)
            {
                Error = error;
                VisibleQuestions = visibleQuestions;
                RootQuestionIds = rootQuestionIds;
            }

            public Error? Error { get; }

            public IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> VisibleQuestions { get; }

            public IReadOnlyCollection<Guid> RootQuestionIds { get; }

            public static AnswerValidationResult Ok(
                IReadOnlyCollection<AnonymousTemplateQuestionForSubmitDto> visibleQuestions,
                IReadOnlyCollection<Guid> rootQuestionIds)
            {
                return new AnswerValidationResult(
                    error: null,
                    visibleQuestions: visibleQuestions,
                    rootQuestionIds: rootQuestionIds);
            }

            public static AnswerValidationResult Fail(Error error)
            {
                return new AnswerValidationResult(
                    error: error,
                    visibleQuestions: Array.Empty<AnonymousTemplateQuestionForSubmitDto>(),
                    rootQuestionIds: Array.Empty<Guid>());
            }
        }

        private sealed record ScoreCalculationResult(
            int ActualScore,
            int MaxScore,
            decimal ScorePercentage);
    }
}
