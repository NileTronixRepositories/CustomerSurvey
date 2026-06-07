using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSurveyResponseDetails;

internal sealed class GetBranchSurveyResponseDetailsQueryHandler
    : IQueryHandler<GetBranchSurveyResponseDetailsQuery, GetBranchSurveyResponseDetailsResponse>
{
    private const string SurveyVoiceAnswersBasePath = "Media/SurveyVoiceAnswers";

    private readonly IWriteReadRepository<SuperAdmin> _superAdminReadRepository;
    private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetBranchSurveyResponseDetailsQueryHandler(
        IWriteReadRepository<SuperAdmin> superAdminReadRepository,
        ICurrentBranchScopeResolver currentBranchScopeResolver,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        IWriteReadRepository<QuestionOption> questionOptionReadRepository,
        ICurrentUser currentUser)
    {
        _superAdminReadRepository = superAdminReadRepository
            ?? throw new ArgumentNullException(nameof(superAdminReadRepository));

        _currentBranchScopeResolver = currentBranchScopeResolver
            ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _surveyAnswerReadRepository = surveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _questionOptionReadRepository = questionOptionReadRepository
            ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<Result<GetBranchSurveyResponseDetailsResponse>> Handle(
        GetBranchSurveyResponseDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetBranchSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.BranchResponseDetails.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentApplicationUserId = _currentUser.UserId.Value;

        Guid? currentBranchId = null;

        var currentSuperAdminExists = await _superAdminReadRepository.AnyAsync(
            x => x.ApplicationUserId == currentApplicationUserId,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<GetBranchSurveyResponseDetailsResponse>.Fail(
                    currentBranchScope.Errors);
            }

            currentBranchId = currentBranchScope.Value.BranchId;
        }

        var surveyResponse = await _surveyResponseReadRepository.FirstOrDefaultAsync(
            new GetBranchSurveyResponseDetailsBasicSpec(
                request.SurveyResponseId,
                currentBranchId),
            cancellationToken);

        if (surveyResponse is null)
        {
            return Result<GetBranchSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.BranchResponseDetails.ResponseNotFound",
                Message: ErrorMessage.GetBranchSurveyResponseDetails_Response_NotFound,
                Type: ErrorType.NotFound));
        }

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetBranchSurveyResponseAnswersSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetBranchSurveyResponseCustomInputValuesSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var selectedOptionIds = answers
            .Where(x =>
                x.QuestionType == QuestionType.SingleChoice &&
                x.SelectedQuestionOptionId.HasValue &&
                x.SelectedQuestionOptionId.Value != Guid.Empty)
            .Select(x => x.SelectedQuestionOptionId!.Value)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<Guid, QuestionOptionForSurveyResponseDetailsDto> optionsById;

        if (selectedOptionIds.Length == 0)
        {
            optionsById = new Dictionary<Guid, QuestionOptionForSurveyResponseDetailsDto>();
        }
        else
        {
            var options = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForSurveyResponseDetailsSpec(selectedOptionIds),
                cancellationToken);

            optionsById = options.ToDictionary(x => x.OptionId);
        }

        var response = new GetBranchSurveyResponseDetailsResponse
        {
            SurveyResponseId = surveyResponse.SurveyResponseId,
            TemplateId = surveyResponse.TemplateId,
            TemplateNameEn = surveyResponse.TemplateNameEn,
            TemplateNameAr = surveyResponse.TemplateNameAr,
            OperatorId = surveyResponse.OperatorId,
            OperatorNameEn = surveyResponse.OperatorNameEn,
            OperatorNameAr = surveyResponse.OperatorNameAr,
            SubmittedOnUtc = surveyResponse.SubmittedOnUtc,

            Score = new BranchSurveyResponseScoreResponse
            {
                ActualScore = surveyResponse.ActualScore,
                MaxScore = surveyResponse.MaxScore,
                ScorePercentage = surveyResponse.ScorePercentage,
                IsScored = surveyResponse.MaxScore > 0
            },

            CustomInputsCount = customInputValues.Count,
            AnswersCount = answers.Count,

            CustomInputs = customInputValues
                .Select(MapCustomInput)
                .ToArray(),

            Answers = answers
                .Select(answer => MapAnswer(answer, optionsById))
                .ToArray()
        };

        return Result<GetBranchSurveyResponseDetailsResponse>.Ok(response);
    }

    private static BranchSurveyResponseCustomInputResponse MapCustomInput(
        BranchSurveyResponseCustomInputValueDto customInput)
    {
        var displayValue = customInput.TypeSnapshot switch
        {
            TemplateCustomInputType.String => customInput.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => customInput.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };

        return new BranchSurveyResponseCustomInputResponse
        {
            CustomInputId = customInput.CustomInputId,
            Name = customInput.NameSnapshot,
            Type = customInput.TypeSnapshot,
            TypeName = customInput.TypeSnapshot.ToString(),
            StringValue = customInput.StringValue,
            IntegerValue = customInput.IntegerValue,
            DisplayValue = displayValue
        };
    }

    private static BranchSurveyResponseAnswerResponse MapAnswer(
        BranchSurveyAnswerDetailsDto answer,
        IReadOnlyDictionary<Guid, QuestionOptionForSurveyResponseDetailsDto> optionsById)
    {
        QuestionOptionForSurveyResponseDetailsDto? selectedOption = null;

        if (answer.SelectedQuestionOptionId.HasValue)
        {
            optionsById.TryGetValue(
                answer.SelectedQuestionOptionId.Value,
                out selectedOption);
        }

        var voiceFileUrl = !string.IsNullOrWhiteSpace(answer.VoiceFileName)
            ? $"{SurveyVoiceAnswersBasePath}/{answer.VoiceFileName}"
            : null;

        return new BranchSurveyResponseAnswerResponse
        {
            QuestionId = answer.QuestionId,
            QuestionTextEn = answer.QuestionTextEn,
            QuestionTextAr = answer.QuestionTextAr,
            QuestionType = answer.QuestionType,
            QuestionTypeName = answer.QuestionType.ToString(),

            SelectedQuestionOptionId = answer.SelectedQuestionOptionId,
            SelectedOptionTextEn = selectedOption?.TextEn,
            SelectedOptionTextAr = selectedOption?.TextAr,
            SelectedOptionValue = selectedOption?.Value,

            StarRatingValue = answer.StarRatingValue,
            SmileValue = answer.SmileValue,
            TextAnswer = answer.TextAnswer,
            VoiceFileName = answer.VoiceFileName,
            VoiceFileUrl = voiceFileUrl,

            DisplayValue = ResolveDisplayValue(
                answer,
                selectedOption,
                voiceFileUrl)
        };
    }

    private static string ResolveDisplayValue(
        BranchSurveyAnswerDetailsDto answer,
        QuestionOptionForSurveyResponseDetailsDto? selectedOption,
        string? voiceFileUrl)
    {
        return answer.QuestionType switch
        {
            QuestionType.SingleChoice => selectedOption?.TextEn ?? string.Empty,
            QuestionType.StarRating => answer.StarRatingValue?.ToString() ?? string.Empty,
            QuestionType.Smiles => answer.SmileValue?.ToString() ?? string.Empty,
            QuestionType.Complain => answer.TextAnswer ?? string.Empty,
            QuestionType.Voice => voiceFileUrl ?? answer.VoiceFileName ?? string.Empty,
            _ => string.Empty
        };
    }
}
