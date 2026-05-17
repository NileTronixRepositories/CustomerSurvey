using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using BuildingBlock.Domain.SharedDto;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesPagination
{
    internal sealed class GetTemplatesPaginationQueryHandler
          : IQueryHandler<GetTemplatesPaginationQuery, Pagination<TemplatePaginationItemResponse>>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentUser _currentUser;

        public GetTemplatesPaginationQueryHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentUser currentUser)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<Pagination<TemplatePaginationItemResponse>>> Handle(
            GetTemplatesPaginationQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<Pagination<TemplatePaginationItemResponse>>.Fail(new Error(
                    Code: "Templates.Pagination.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            request.SearchText ??= string.Empty;

            var currentApplicationUserId = _currentUser.UserId.Value;

            var actorBranchIdResult = await ResolveCurrentActorBranchIdAsync(
                currentApplicationUserId,
                cancellationToken);

            if (actorBranchIdResult.IsFailure)
            {
                return Result<Pagination<TemplatePaginationItemResponse>>.Fail(
                    actorBranchIdResult.Errors);
            }

            var branchId = actorBranchIdResult.Value;

            var spec = new GetTemplatesPaginationSpec(
                branchId,
                request,
                request.IsActive);

            var (items, totalCount) = await _templateReadRepository.ListWithCountAsync(
                spec,
                cancellationToken);

            var responseItems = items
                .Select(x => new TemplatePaginationItemResponse
                {
                    TemplateId = x.TemplateId,
                    BranchId = x.BranchId,
                    NameEn = x.NameEn,
                    NameAr = x.NameAr,
                    Description = x.Description,
                    Status = x.Status.ToString(),
                    IsActive = x.IsActive,
                    QuestionsCount = x.QuestionsCount,
                    CreatedOnUtc = x.CreatedOnUtc,
                    ActiveFrom = x.ActiveFrom,
                    ExpireTo = x.ExpireTo
                })
                .ToArray();

            var response = new Pagination<TemplatePaginationItemResponse>(
                currentPage: request.PageNumber,
                pageSize: request.PageSize,
                totalItems: totalCount,
                data: responseItems);

            return Result<Pagination<TemplatePaginationItemResponse>>.Ok(response);
        }

        private async Task<Result<Guid>> ResolveCurrentActorBranchIdAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForTemplatesPaginationSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                return Result<Guid>.Ok(branchAdmin.BranchId);
            }

            var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchUserForTemplatesPaginationSpec(currentApplicationUserId),
                cancellationToken);

            if (branchUser is not null)
            {
                return Result<Guid>.Ok(branchUser.BranchId);
            }

            return Result<Guid>.Fail(new Error(
                Code: "Templates.Pagination.CurrentBranchActorNotFound",
                Message: ErrorMessage.GetTemplatesPagination_CurrentBranchActor_NotFound,
                Type: ErrorType.Security));
        }
    }
}