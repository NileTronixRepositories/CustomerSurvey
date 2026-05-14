using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Questions.Shared;
using CustomerSurvey.Application.Features.Questions.Shared.Specs;
using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateQuestionsSelection
{
    internal sealed class GetTemplateQuestionsSelectionQueryHandler
        : IQueryHandler<GetTemplateQuestionsSelectionQuery, GetTemplateQuestionsSelectionResponse>
    {
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly IWriteReadRepository<QuestionOption> _questionOptionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestion> _templateQuestionReadRepository;
        private readonly IWriteReadRepository<TemplateQuestionCondition> _conditionReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetTemplateQuestionsSelectionQueryHandler(
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            IWriteReadRepository<QuestionOption> questionOptionReadRepository,
            IWriteReadRepository<TemplateQuestionCondition> conditionReadRepository,
            IWriteReadRepository<TemplateQuestion> templateQuestionReadRepository,
            ICurrentUser currentUser)
        {
            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _questionOptionReadRepository = questionOptionReadRepository
                ?? throw new ArgumentNullException(nameof(questionOptionReadRepository));

            _conditionReadRepository = conditionReadRepository
                ?? throw new ArgumentNullException(nameof(conditionReadRepository));

            _templateQuestionReadRepository = templateQuestionReadRepository
                ?? throw new ArgumentNullException(nameof(templateQuestionReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetTemplateQuestionsSelectionResponse>> Handle(
            GetTemplateQuestionsSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "Templates.QuestionsSelection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentActor = await ResolveCurrentBranchActorAsync(
                currentApplicationUserId,
                cancellationToken);

            if (currentActor is null)
            {
                return Result<GetTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "Templates.QuestionsSelection.CurrentBranchActorNotFound",
                    Message: ErrorMessage.GetTemplateQuestionsSelection_CurrentBranchActor_NotFound,
                    Type: ErrorType.NotFound));
            }

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForQuestionsSelectionSpec(
                    templateId: request.TemplateId,
                    branchId: currentActor.BranchId),
                cancellationToken);

            if (template is null)
            {
                return Result<GetTemplateQuestionsSelectionResponse>.Fail(new Error(
                    Code: "Templates.QuestionsSelection.TemplateNotFound",
                    Message: ErrorMessage.GetTemplateQuestionsSelection_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            var groups = await _questionGroupReadRepository.ListAsync(
                new GetQuestionGroupsForTemplateQuestionsSelectionSpec(currentActor.BranchId),
                cancellationToken);

            var questions = await _questionReadRepository.ListAsync(
                new GetQuestionsForTemplateQuestionsSelectionSpec(currentActor.BranchId),
                cancellationToken);

            var templateQuestions = await _templateQuestionReadRepository.ListAsync(
                new GetTemplateQuestionsForTemplateQuestionsSelectionSpec(template.TemplateId),
                cancellationToken);

            var questionConditions = await _conditionReadRepository.ListAsync(
                new GetTemplateQuestionConditionsByTemplateIdsSpec(
                    new[] { template.TemplateId }),
                cancellationToken);

            var validQuestionConditions = FilterValidConditions(
                templateQuestions,
                questionConditions);

            var singleChoiceQuestionIds = questions
     .Where(x => x.Type == QuestionType.SingleChoice)
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

            var optionsByQuestionId = questionOptions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<TemplateQuestionSelectionOptionResponse>)x
                        .OrderBy(option => option.Order)
                        .Select(option => new TemplateQuestionSelectionOptionResponse
                        {
                            OptionId = option.OptionId,
                            TextEn = option.TextEn,
                            TextAr = option.TextAr,
                            Order = option.Order
                        })
                        .ToArray());

            var selectedQuestionsByQuestionId = templateQuestions
                .GroupBy(x => x.QuestionId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderBy(item => item.Order)
                        .First());

            var questionsByGroupId = questions
                .GroupBy(x => x.GroupId)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .Select(question =>
                        {
                            var isSelected = selectedQuestionsByQuestionId.TryGetValue(
                                question.QuestionId,
                                out var selectedTemplateQuestion);

                            return new TemplateQuestionsSelectionQuestionResponse
                            {
                                QuestionId = question.QuestionId,

                                TemplateQuestionId = isSelected
                                    ? selectedTemplateQuestion!.TemplateQuestionId
                                    : null,

                                TextEn = question.TextEn,
                                TextAr = question.TextAr,
                                Type = question.Type.ToString(),
                                IsSelected = isSelected,

                                Order = isSelected
                                    ? selectedTemplateQuestion!.Order
                                    : null,

                                Options = question.Type == QuestionType.SingleChoice
                                          && optionsByQuestionId.TryGetValue(
                                              question.QuestionId,
                                              out var options)
                                    ? options
                                    : Array.Empty<TemplateQuestionSelectionOptionResponse>()
                            };
                        })
                        .OrderByDescending(question => question.IsSelected)
                        .ThenBy(question => question.Order ?? int.MaxValue)
                        .ThenBy(question => question.TextEn)
                        .ToArray());

            var response = new GetTemplateQuestionsSelectionResponse
            {
                TemplateId = template.TemplateId,
                BranchId = template.BranchId,
                TemplateNameEn = template.NameEn,
                TemplateNameAr = template.NameAr,
                Status = template.Status.ToString(),
                IsActive = template.IsActive,

                Groups = groups
                    .Select(group => new TemplateQuestionsSelectionGroupResponse
                    {
                        GroupId = group.GroupId,
                        NameEn = group.NameEn,
                        NameAr = group.NameAr,
                        Questions = questionsByGroupId.TryGetValue(
                            group.GroupId,
                            out var groupQuestions)
                                ? groupQuestions
                                : Array.Empty<TemplateQuestionsSelectionQuestionResponse>()
                    })
                    .ToArray(),

                QuestionConditions = validQuestionConditions
                    .OrderBy(condition => condition.Order)
                    .ThenBy(condition => condition.ParentTemplateQuestionId)
                    .ThenBy(condition => condition.ChildTemplateQuestionId)
                    .Select(condition => condition.ToResponse())
                    .ToArray()
            };

            return Result<GetTemplateQuestionsSelectionResponse>.Ok(response);
        }

        private async Task<CurrentBranchActorForTemplateQuestionsSelectionDto?> ResolveCurrentBranchActorAsync(
            Guid applicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForTemplateQuestionsSelectionSpec(applicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return branchAdmin;
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForTemplateQuestionsSelectionSpec(applicationUserId),
                cancellationToken);

            return branchUser;
        }

        private static TemplateQuestionConditionForReadDto[] FilterValidConditions(
     IReadOnlyCollection<TemplateQuestionForTemplateQuestionsSelectionDto> templateQuestions,
     IReadOnlyCollection<TemplateQuestionConditionForReadDto> conditions)
        {
            if (templateQuestions.Count == 0 || conditions.Count == 0)
            {
                return Array.Empty<TemplateQuestionConditionForReadDto>();
            }

            var selectedTemplateQuestionIds = templateQuestions
                .Select(question => question.TemplateQuestionId)
                .ToHashSet();

            return conditions
                .Where(condition =>
                    selectedTemplateQuestionIds.Contains(condition.ParentTemplateQuestionId) &&
                    selectedTemplateQuestionIds.Contains(condition.ChildTemplateQuestionId))
                .OrderBy(condition => condition.Order)
                .ThenBy(condition => condition.ParentTemplateQuestionId)
                .ThenBy(condition => condition.ChildTemplateQuestionId)
                .ToArray();
        }
    }
}