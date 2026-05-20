using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentOperatorSurveyResponseDetails;

using DomainOperator = CustomerSurvey.Domain.Identity.Operator;

internal sealed class GetDepartmentOperatorSurveyResponseDetailsQueryHandler
    : IQueryHandler<GetDepartmentOperatorSurveyResponseDetailsQuery, GetDepartmentOperatorSurveyResponseDetailsResponse>
{
    private const string SurveyVoiceAnswersBasePath = "Media/SurveyVoiceAnswers";

    private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
    private readonly IWriteReadRepository<DomainOperator> _operatorReadRepository;
    private readonly IWriteReadRepository<SurveyResponse> _surveyResponseReadRepository;
    private readonly IWriteReadRepository<SurveyAnswer> _surveyAnswerReadRepository;
    private readonly IWriteReadRepository<SurveyResponseCustomInputValue> _customInputValueReadRepository;
    private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
    private readonly ICurrentUser _currentUser;

    public GetDepartmentOperatorSurveyResponseDetailsQueryHandler(
        IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
        IWriteReadRepository<DomainOperator> operatorReadRepository,
        IWriteReadRepository<SurveyResponse> surveyResponseReadRepository,
        IWriteReadRepository<SurveyAnswer> surveyAnswerReadRepository,
        IWriteReadRepository<SurveyResponseCustomInputValue> customInputValueReadRepository,
        IWriteReadRepository<QuestionOption> questionOptionReadRepository,
        ICurrentUser currentUser)
    {
        _departmentAdminReadRepository = departmentAdminReadRepository
            ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

        _operatorReadRepository = operatorReadRepository
            ?? throw new ArgumentNullException(nameof(operatorReadRepository));

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

    public async Task<Result<GetDepartmentOperatorSurveyResponseDetailsResponse>> Handle(
        GetDepartmentOperatorSurveyResponseDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
        {
            return Result<GetDepartmentOperatorSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponseDetails.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentDepartmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
            new GetCurrentDepartmentAdminForDepartmentOperatorResponseDetailsSpec(_currentUser.UserId.Value),
            cancellationToken);

        if (currentDepartmentAdmin is null)
        {
            return Result<GetDepartmentOperatorSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponseDetails.CurrentDepartmentAdminNotFound",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponseDetails_CurrentDepartmentAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        var operatorProfile = await _operatorReadRepository.FirstOrDefaultAsync(
            new GetDepartmentOperatorForResponseDetailsSpec(
                request.OperatorId,
                currentDepartmentAdmin.DepartmentId),
            cancellationToken);

        if (operatorProfile is null)
        {
            return Result<GetDepartmentOperatorSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponseDetails.OperatorNotFound",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponseDetails_Operator_NotFound,
                Type: ErrorType.NotFound));
        }

        var surveyResponse = await _surveyResponseReadRepository.FirstOrDefaultAsync(
            new GetDepartmentOperatorSurveyResponseDetailsBasicSpec(
                request.SurveyResponseId,
                operatorProfile.OperatorId,
                currentDepartmentAdmin.DepartmentId),
            cancellationToken);

        if (surveyResponse is null)
        {
            return Result<GetDepartmentOperatorSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.DepartmentOperatorResponseDetails.ResponseNotFound",
                Message: ErrorMessage.GetDepartmentOperatorSurveyResponseDetails_Response_NotFound,
                Type: ErrorType.NotFound));
        }

        var answers = await _surveyAnswerReadRepository.ListAsync(
            new GetDepartmentOperatorSurveyResponseAnswersSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var customInputValues = await _customInputValueReadRepository.ListAsync(
            new GetDepartmentOperatorSurveyResponseCustomInputValuesSpec(surveyResponse.SurveyResponseId),
            cancellationToken);

        var selectedOptionIds = answers
            .Where(x =>
                x.QuestionType == QuestionType.SingleChoice &&
                x.SelectedQuestionOptionId.HasValue &&
                x.SelectedQuestionOptionId.Value != Guid.Empty)
            .Select(x => x.SelectedQuestionOptionId!.Value)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<Guid, QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto> optionsById;

        if (selectedOptionIds.Length == 0)
        {
            optionsById = new Dictionary<Guid, QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto>();
        }
        else
        {
            var options = await _questionOptionReadRepository.ListAsync(
                new GetQuestionOptionsForDepartmentOperatorSurveyResponseDetailsSpec(selectedOptionIds),
                cancellationToken);

            optionsById = options.ToDictionary(x => x.OptionId);
        }

        var response = new GetDepartmentOperatorSurveyResponseDetailsResponse
        {
            SurveyResponseId = surveyResponse.SurveyResponseId,
            Branch = new DepartmentOperatorSurveyResponseBranchInfoResponse
            {
                BranchId = surveyResponse.BranchId,
                NameEn = surveyResponse.BranchNameEn,
                NameAr = surveyResponse.BranchNameAr,
                Code = surveyResponse.BranchCode
            },
            Template = new DepartmentOperatorSurveyResponseTemplateInfoResponse
            {
                TemplateId = surveyResponse.TemplateId,
                NameEn = surveyResponse.TemplateNameEn,
                NameAr = surveyResponse.TemplateNameAr
            },
            Operator = new DepartmentOperatorSurveyResponseOperatorInfoResponse
            {
                OperatorId = surveyResponse.OperatorId,
                NameEn = surveyResponse.OperatorNameEn,
                NameAr = surveyResponse.OperatorNameAr
            },
            SubmittedOnUtc = surveyResponse.SubmittedOnUtc,

            Score = new DepartmentOperatorSurveyResponseScoreResponse
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

        return Result<GetDepartmentOperatorSurveyResponseDetailsResponse>.Ok(response);
    }

    private static DepartmentOperatorSurveyResponseCustomInputResponse MapCustomInput(
        DepartmentOperatorSurveyResponseCustomInputValueDto customInput)
    {
        var displayValue = customInput.TypeSnapshot switch
        {
            TemplateCustomInputType.String => customInput.StringValue ?? string.Empty,
            TemplateCustomInputType.Integer => customInput.IntegerValue?.ToString() ?? string.Empty,
            _ => string.Empty
        };

        return new DepartmentOperatorSurveyResponseCustomInputResponse
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

    private static DepartmentOperatorSurveyResponseAnswerResponse MapAnswer(
        DepartmentOperatorSurveyAnswerDetailsDto answer,
        IReadOnlyDictionary<Guid, QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto> optionsById)
    {
        QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto? selectedOption = null;

        if (answer.SelectedQuestionOptionId.HasValue)
        {
            optionsById.TryGetValue(
                answer.SelectedQuestionOptionId.Value,
                out selectedOption);
        }

        var voiceFileUrl = !string.IsNullOrWhiteSpace(answer.VoiceFileName)
            ? $"{SurveyVoiceAnswersBasePath}/{answer.VoiceFileName}"
            : null;

        return new DepartmentOperatorSurveyResponseAnswerResponse
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
        DepartmentOperatorSurveyAnswerDetailsDto answer,
        QuestionOptionForDepartmentOperatorSurveyResponseDetailsDto? selectedOption,
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
