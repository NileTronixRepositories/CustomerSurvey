using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Media;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Common;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private readonly IWriteReadRepository<Operator> _operatorReadRepository;
        private readonly IWriteReadRepository<OperatorTemplate> _operatorTemplateReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteRepository<SurveyResponse> _surveyResponseWriteRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IMediaService _mediaService;
        private readonly IUnitOfWork _unitOfWork;

        public SubmitOperatorTemplateResponseCommandHandler(
            IWriteReadRepository<Operator> operatorReadRepository,
            IWriteReadRepository<OperatorTemplate> operatorTemplateReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteRepository<SurveyResponse> surveyResponseWriteRepository,
            ICurrentUser currentUser,
            IMediaService mediaService,
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

            _surveyResponseWriteRepository = surveyResponseWriteRepository
                ?? throw new ArgumentNullException(nameof(surveyResponseWriteRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _mediaService = mediaService
                ?? throw new ArgumentNullException(nameof(mediaService));

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
                    currentOperator.OperatorId,
                    request.TemplateId),
                cancellationToken);

            if (assignedTemplate is null)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.TemplateNotAssignedToOperator",
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

            var hasUnknownQuestion = submittedQuestionIds.Any(x => !templateQuestionIds.Contains(x));

            if (hasUnknownQuestion)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.QuestionNotInTemplate",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Question_NotInTemplate,
                    Type: ErrorType.Validation));
            }

            var allTemplateQuestionsAnswered = templateQuestionIds.All(x => submittedQuestionIds.Contains(x));

            if (!allTemplateQuestionsAnswered)
            {
                return Result<SubmitOperatorTemplateResponseResponse>.Fail(new Error(
                    Code: "SurveyResponses.Submit.AllQuestionsRequired",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_AllQuestions_Required,
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

            var surveyResponse = SurveyResponse.Create(
                operatorId: currentOperator.OperatorId,
                templateId: request.TemplateId,
                createdByApplicationUserId: currentApplicationUserId);

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
                SubmittedOnUtc = surveyResponse.SubmittedOnUtc
            };

            return Result<SubmitOperatorTemplateResponseResponse>.Ok(response);
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
                QuestionType.SingleChoice => ValidateSingleChoice(answer, optionsByQuestionId),
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

            if (!answer.SelectedQuestionOptionId.HasValue)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SingleChoiceOptionRequired",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_SingleChoice_OptionRequired,
                    Type: ErrorType.Validation);
            }

            if (!optionsByQuestionId.TryGetValue(answer.QuestionId, out var validOptionIds) ||
                !validOptionIds.Contains(answer.SelectedQuestionOptionId.Value))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SingleChoiceOptionInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_SingleChoice_OptionInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateVoice(SubmitOperatorTemplateAnswerCommandItem answer)
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

        private static Error? ValidateStarRating(SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlyStarRatingFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.StarRatingInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_StarRating_InvalidShape,
                    Type: ErrorType.Validation);
            }

            if (!answer.StarRatingValue.HasValue ||
                answer.StarRatingValue.Value < 1 ||
                answer.StarRatingValue.Value > 5)
            {
                return new Error(
                    Code: "SurveyResponses.Submit.StarRatingValueInvalid",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_StarRating_ValueInvalid,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateComplain(SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlyComplainFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.ComplainInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Complain_InvalidShape,
                    Type: ErrorType.Validation);
            }

            if (string.IsNullOrWhiteSpace(answer.TextAnswer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.ComplainTextRequired",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Complain_TextRequired,
                    Type: ErrorType.Validation);
            }

            return null;
        }

        private static Error? ValidateSmiles(SubmitOperatorTemplateAnswerCommandItem answer)
        {
            if (!OnlySmilesFieldsProvided(answer))
            {
                return new Error(
                    Code: "SurveyResponses.Submit.SmilesInvalidShape",
                    Message: ErrorMessage.SubmitOperatorTemplateResponse_Smiles_InvalidShape,
                    Type: ErrorType.Validation);
            }

            if (!answer.SmileValue.HasValue ||
                answer.SmileValue.Value < 1 ||
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
                   answer.StarRatingValue is null &&
                   answer.SmileValue is null &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlyVoiceFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return answer.SelectedQuestionOptionId is null &&
                   answer.StarRatingValue is null &&
                   answer.SmileValue is null &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is not null;
        }

        private static bool OnlyStarRatingFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return answer.SelectedQuestionOptionId is null &&
                   answer.StarRatingValue.HasValue &&
                   answer.SmileValue is null &&
                   string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlyComplainFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return answer.SelectedQuestionOptionId is null &&
                   answer.StarRatingValue is null &&
                   answer.SmileValue is null &&
                   !string.IsNullOrWhiteSpace(answer.TextAnswer) &&
                   answer.VoiceFile is null;
        }

        private static bool OnlySmilesFieldsProvided(
            SubmitOperatorTemplateAnswerCommandItem answer)
        {
            return answer.SelectedQuestionOptionId is null &&
                   answer.StarRatingValue is null &&
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
    }
}