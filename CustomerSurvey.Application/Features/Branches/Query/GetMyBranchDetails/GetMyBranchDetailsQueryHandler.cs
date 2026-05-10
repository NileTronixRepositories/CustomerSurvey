using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetMyBranchDetails
{
    internal sealed class GetMyBranchDetailsQueryHandler
         : IQueryHandler<GetMyBranchDetailsQuery, GetBranchDetailsResponse>
    {
        private readonly IWriteReadRepository<Branch> _branchReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<UserRole> _userRoleReadRepository;
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<QuestionGroup> _questionGroupReadRepository;
        private readonly IWriteReadRepository<Question> _questionReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetMyBranchDetailsQueryHandler(
            IWriteReadRepository<Branch> branchReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<UserRole> userRoleReadRepository,
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<QuestionGroup> questionGroupReadRepository,
            IWriteReadRepository<Question> questionReadRepository,
            ICurrentUser currentUser)
        {
            _branchReadRepository = branchReadRepository
                ?? throw new ArgumentNullException(nameof(branchReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _userRoleReadRepository = userRoleReadRepository
                ?? throw new ArgumentNullException(nameof(userRoleReadRepository));

            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _questionGroupReadRepository = questionGroupReadRepository
                ?? throw new ArgumentNullException(nameof(questionGroupReadRepository));

            _questionReadRepository = questionReadRepository
                ?? throw new ArgumentNullException(nameof(questionReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<GetBranchDetailsResponse>> Handle(
            GetMyBranchDetailsQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<GetBranchDetailsResponse>.Fail(new Error(
                    Code: "Branches.MyBranch.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForMyBranchDetailsSpec(currentApplicationUserId),
                cancellationToken);

            if (currentBranchAdmin is null)
            {
                return Result<GetBranchDetailsResponse>.Fail(new Error(
                    Code: "Branches.MyBranch.CurrentBranchAdminNotFound",
                    Message: ErrorMessage.GetMyBranchDetails_CurrentBranchAdmin_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branchId = currentBranchAdmin.BranchId;

            var branch = await _branchReadRepository.FirstOrDefaultAsync(
                new GetBranchBasicDetailsSpec(branchId),
                cancellationToken);

            if (branch is null)
            {
                return Result<GetBranchDetailsResponse>.Fail(new Error(
                    Code: "Branches.MyBranch.BranchNotFound",
                    Message: ErrorMessage.GetMyBranchDetails_Branch_NotFound,
                    Type: ErrorType.NotFound));
            }

            var branchAdmins = await _branchAdminReadRepository.ListAsync(
                new GetBranchAdminsForBranchDetailsSpec(branchId),
                cancellationToken);

            var branchUsers = await _branchUserReadRepository.ListAsync(
                new GetBranchUsersForBranchDetailsSpec(branchId),
                cancellationToken);

            var branchUserApplicationUserIds = branchUsers
                .Select(x => x.ApplicationUserId)
                .Distinct()
                .ToArray();

            IReadOnlyCollection<BranchUserRoleForBranchDetailsDto> branchUserRoles;

            if (branchUserApplicationUserIds.Length == 0)
            {
                branchUserRoles = Array.Empty<BranchUserRoleForBranchDetailsDto>();
            }
            else
            {
                branchUserRoles = await _userRoleReadRepository.ListAsync(
                    new GetBranchUserRolesForBranchDetailsSpec(branchUserApplicationUserIds),
                    cancellationToken);
            }

            var templates = await _templateReadRepository.ListAsync(
                new GetTemplatesForBranchDetailsSpec(branchId),
                cancellationToken);

            var questionGroups = await _questionGroupReadRepository.ListAsync(
                new GetQuestionGroupsForBranchDetailsSpec(branchId),
                cancellationToken);

            var questions = await _questionReadRepository.ListAsync(
                new GetQuestionsForBranchDetailsSpec(branchId),
                cancellationToken);

            var rolesByApplicationUserId = branchUserRoles
                .GroupBy(x => x.ApplicationUserId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<BranchDetailsUserRoleResponse>)x
                        .Select(role => new BranchDetailsUserRoleResponse
                        {
                            RoleId = role.RoleId,
                            Name = role.RoleName
                        })
                        .ToArray());

            var questionsByGroupId = questions
                .GroupBy(x => x.GroupId)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyCollection<BranchDetailsQuestionResponse>)x
                        .Select(question => new BranchDetailsQuestionResponse
                        {
                            QuestionId = question.QuestionId,
                            TextEn = question.TextEn,
                            TextAr = question.TextAr,
                            Type = question.Type.ToString(),
                            IsActive = question.IsActive
                        })
                        .ToArray());

            var response = new GetBranchDetailsResponse
            {
                Id = branch.Id,
                NameEn = branch.NameEn,
                NameAr = branch.NameAr,
                Code = branch.Code,
                Address = branch.Address,
                IsActive = branch.IsActive,
                CreatedOnUtc = branch.CreatedOnUtc,

                Summary = new BranchDetailsSummaryResponse
                {
                    BranchAdminsCount = branchAdmins.Count,
                    BranchUsersCount = branchUsers.Count,
                    TemplatesCount = templates.Count,
                    QuestionGroupsCount = questionGroups.Count,
                    QuestionsCount = questions.Count
                },

                BranchAdmins = branchAdmins,

                BranchUsers = branchUsers
                    .Select(branchUser => new BranchDetailsBranchUserResponse
                    {
                        BranchUserId = branchUser.BranchUserId,
                        ApplicationUserId = branchUser.ApplicationUserId,
                        NameEn = branchUser.NameEn,
                        NameAr = branchUser.NameAr,
                        UserName = branchUser.UserName,
                        Email = branchUser.Email,
                        PhoneNumber = branchUser.PhoneNumber,
                        Roles = rolesByApplicationUserId.TryGetValue(
                            branchUser.ApplicationUserId,
                            out var roles)
                                ? roles
                                : Array.Empty<BranchDetailsUserRoleResponse>()
                    })
                    .ToArray(),

                Templates = templates
                    .Select(template => new BranchDetailsTemplateResponse
                    {
                        TemplateId = template.TemplateId,
                        NameEn = template.NameEn,
                        NameAr = template.NameAr,
                        Description = template.Description,
                        Status = template.Status.ToString(),
                        QuestionsCount = template.QuestionsCount
                    })
                    .ToArray(),

                QuestionGroups = questionGroups
                    .Select(group => new BranchDetailsQuestionGroupResponse
                    {
                        GroupId = group.GroupId,
                        NameEn = group.NameEn,
                        NameAr = group.NameAr,
                        Questions = questionsByGroupId.TryGetValue(
                            group.GroupId,
                            out var groupQuestions)
                                ? groupQuestions
                                : Array.Empty<BranchDetailsQuestionResponse>()
                    })
                    .ToArray()
            };

            return Result<GetBranchDetailsResponse>.Ok(response);
        }
    }
}