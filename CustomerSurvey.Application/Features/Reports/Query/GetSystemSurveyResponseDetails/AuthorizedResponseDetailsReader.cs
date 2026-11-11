using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class AuthorizedResponseDetailsReader
{
    private const string SurveyVoiceAnswersBasePath = "Media/SurveyVoiceAnswers";
    private const string SurveyAnswerImagesBasePath = "Media/SurveyAnswerImages";

    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;

    public AuthorizedResponseDetailsReader(
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        IWriteReadRepository<QuestionOption> questionOptionReadRepository)
    {
        _surveyResponseReadRepository = surveyResponseReadRepository
            ?? throw new ArgumentNullException(nameof(surveyResponseReadRepository));

        _surveyAnswerReadRepository = surveyAnswerReadRepository
            ?? throw new ArgumentNullException(nameof(surveyAnswerReadRepository));

        _customInputValueReadRepository = customInputValueReadRepository
            ?? throw new ArgumentNullException(nameof(customInputValueReadRepository));

        _questionOptionReadRepository = questionOptionReadRepository
            ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));
    }

    public async Task<Result<GetSystemSurveyResponseDetailsResponse>> ReadAsync(
        Guid responseId,
        bool requireFlags,
        CancellationToken cancellationToken)
    {
        var surveyResponse = await _surveyResponseReadRepository.FirstOrDefaultAsync(
            new GetSystemSurveyResponseDetailsBasicSpec(responseId, requireFlags),
            cancellationToken);

        if (surveyResponse is null)
        {
            return Result<GetSystemSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.SystemResponseDetails.ResponseNotFound",
                Message: ErrorMessage.GetSystemSurveyResponseDetails_Response_NotFound,
                Type: ErrorType.NotFound));
        }

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetSystemSurveyResponseAnswersSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetSystemSurveyResponseCustomInputValuesSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var historicalSurvey = (await _surveyResponseReadRepository.FirstOrDefaultAsync(
            new CustomerSurvey.Application.Features.Reports.Shared.GetAuthorizedResponseSnapshotSpec(surveyResponse.SurveyResponseId),
            cancellationToken))?.Read();

        var selectedOptionIds = answers
            .Where(x =>
                x.QuestionType == QuestionType.SingleChoice &&
                x.SelectedQuestionOptionId.HasValue &&
                x.SelectedQuestionOptionId.Value != Guid.Empty)
            .Select(x => x.SelectedQuestionOptionId!.Value)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<Guid, QuestionOptionForSystemSurveyResponseDetailsDto> optionsById;

        if (selectedOptionIds.Length == 0)
        {
            optionsById = new Dictionary<Guid, QuestionOptionForSystemSurveyResponseDetailsDto>();
        }
        else
        {
            var options = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForSystemSurveyResponseDetailsSpec(selectedOptionIds),
                cancellationToken);

            optionsById = options.ToDictionary(x => x.OptionId);
        }

        var response = new GetSystemSurveyResponseDetailsResponse
        {
            HistoricalSurvey = historicalSurvey,
            SurveyResponseId = surveyResponse.SurveyResponseId,

            Branch = new SystemSurveyResponseBranchInfoResponse
            {
                BranchId = surveyResponse.BranchId,
                NameEn = surveyResponse.BranchNameEn,
                NameAr = surveyResponse.BranchNameAr,
                Code = surveyResponse.BranchCode
            },

            Department = new SystemSurveyResponseDepartmentInfoResponse
            {
                DepartmentId = surveyResponse.DepartmentId,
                NameEn = surveyResponse.DepartmentNameEn,
                NameAr = surveyResponse.DepartmentNameAr
            },

            Template = new SystemSurveyResponseTemplateInfoResponse
            {
                TemplateId = surveyResponse.TemplateId,
                NameEn = !string.IsNullOrEmpty(historicalSurvey?.TemplateNameEn) ? historicalSurvey.TemplateNameEn : surveyResponse.TemplateNameEn,
                NameAr = historicalSurvey is not null ? historicalSurvey.TemplateNameAr : surveyResponse.TemplateNameAr
            },

            Operator = new SystemSurveyResponseOperatorInfoResponse
            {
                OperatorId = surveyResponse.OperatorId,
                NameEn = surveyResponse.OperatorNameEn,
                NameAr = surveyResponse.OperatorNameAr
            },

            SubmittedOnUtc = surveyResponse.SubmittedOnUtc,

            Score = new SystemSurveyResponseScoreResponse
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
                .Select(answer =>
                {
                    var mapped = MapAnswer(answer, optionsById);
                    var saved = historicalSurvey?.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                    return saved is null ? mapped : mapped with
                    {
                        QuestionTextEn = saved.TextEn, QuestionTextAr = saved.TextAr, QuestionType = saved.Type,
                        QuestionTypeName = saved.Type.ToString(), SelectedOptionTextEn = saved.SelectedOptionTextEn,
                        SelectedOptionTextAr = saved.SelectedOptionTextAr, SelectedOptionValue = saved.SelectedOptionValue,
                        DisplayValue = saved.SelectedOptionTextEn ?? mapped.DisplayValue
                    };
                })
                .ToArray()
        };

        return Result<GetSystemSurveyResponseDetailsResponse>.Ok(response);
    }

    private static SystemSurveyResponseCustomInputResponse MapCustomInput(
        SystemSurveyResponseCustomInputValueDto customInput)
    {
        var displayValue = customInput.TypeSnapshot switch
        {
            TemplateCustomInputType.String => customInput.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => customInput.IntegerValue?.ToString() ?? string.Empty,
            TemplateCustomInputType.Date => CustomInputDate.Format(customInput.DateValue),
            _ => string.Empty
        };

        return new SystemSurveyResponseCustomInputResponse
        {
            CustomInputId = customInput.CustomInputId,
            LabelEnSnapshot = customInput.LabelEnSnapshot,
            LabelArSnapshot = customInput.LabelArSnapshot,
            Type = customInput.TypeSnapshot,
            TypeName = customInput.TypeSnapshot.ToString(),
            StringValue = customInput.StringValue,
            IntegerValue = customInput.IntegerValue,
            DateValue = customInput.DateValue,
            DisplayValue = displayValue
        };
    }

    private static SystemSurveyResponseAnswerResponse MapAnswer(
        SystemSurveyAnswerDetailsDto answer,
        IReadOnlyDictionary<Guid, QuestionOptionForSystemSurveyResponseDetailsDto> optionsById)
    {
        QuestionOptionForSystemSurveyResponseDetailsDto? selectedOption = null;

        if (answer.SelectedQuestionOptionId.HasValue)
        {
            optionsById.TryGetValue(
                answer.SelectedQuestionOptionId.Value,
                out selectedOption);
        }

        var voiceFileUrl = !string.IsNullOrWhiteSpace(answer.VoiceFileName)
            ? $"{SurveyVoiceAnswersBasePath}/{answer.VoiceFileName}"
            : null;

        var imageFileUrl = !string.IsNullOrWhiteSpace(answer.ImageFileName)
            ? $"{SurveyAnswerImagesBasePath}/{answer.ImageFileName}"
            : null;

        return new SystemSurveyResponseAnswerResponse
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
            ImageFileName = answer.ImageFileName,
            ImageFileUrl = imageFileUrl,

            DisplayValue = ResolveDisplayValue(
                answer,
                selectedOption,
                voiceFileUrl,
                imageFileUrl)
        };
    }

    private static string ResolveDisplayValue(
        SystemSurveyAnswerDetailsDto answer,
        QuestionOptionForSystemSurveyResponseDetailsDto? selectedOption,
        string? voiceFileUrl,
        string? imageFileUrl)
    {
        return answer.QuestionType switch
        {
            QuestionType.SingleChoice => selectedOption?.TextEn ?? string.Empty,
            QuestionType.StarRating => answer.StarRatingValue?.ToString() ?? string.Empty,
            QuestionType.Smiles => answer.SmileValue?.ToString() ?? string.Empty,
            QuestionType.Complain or QuestionType.FreeText => answer.TextAnswer ?? string.Empty,
            QuestionType.Voice => voiceFileUrl ?? answer.VoiceFileName ?? string.Empty,
            QuestionType.Image => imageFileUrl ?? answer.ImageFileName ?? string.Empty,
            _ => string.Empty
        };
    }
}
