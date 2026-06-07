using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Application.Abstraction.Security;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplatesForSelection
{
    internal sealed class GetTemplatesForSelectionQueryHandler
         : IQueryHandler<GetTemplatesForSelectionQuery, IReadOnlyCollection<TemplateSelectionResponse>>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly IWriteReadRepository<DepartmentAdmin> _departmentAdminReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;

        public GetTemplatesForSelectionQueryHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            IWriteReadRepository<DepartmentAdmin> departmentAdminReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _departmentAdminReadRepository = departmentAdminReadRepository
                ?? throw new ArgumentNullException(nameof(departmentAdminReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<Result<IReadOnlyCollection<TemplateSelectionResponse>>> Handle(
            GetTemplatesForSelectionQuery request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<IReadOnlyCollection<TemplateSelectionResponse>>.Fail(new Error(
                    Code: "Templates.Selection.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var scopeResult = await ResolveSelectionScopeAsync(
                currentApplicationUserId,
                cancellationToken);

            if (scopeResult.IsFailure)
            {
                return Result<IReadOnlyCollection<TemplateSelectionResponse>>.Fail(
                    scopeResult.Errors);
            }

            var spec = new GetTemplatesForSelectionSpec(scopeResult.Value.BranchId);

            var templates = await _templateReadRepository.ListAsync(
                spec,
                cancellationToken);

            return Result<IReadOnlyCollection<TemplateSelectionResponse>>.Ok(templates);
        }

        private async Task<Result<TemplatesSelectionScope>> ResolveSelectionScopeAsync(
            Guid currentApplicationUserId,
            CancellationToken cancellationToken)
        {
            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsSuccess)
            {
                return Result<TemplatesSelectionScope>.Ok(
                    TemplatesSelectionScope.ForBranch(currentBranchScope.Value.BranchId));
            }

            var departmentAdmin = await _departmentAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentDepartmentAdminForTemplatesSelectionSpec(currentApplicationUserId),
                cancellationToken);

            if (departmentAdmin is not null)
            {
                return Result<TemplatesSelectionScope>.Ok(
                    TemplatesSelectionScope.ForAllBranches());
            }

            return Result<TemplatesSelectionScope>.Fail(currentBranchScope.Errors);
        }

        private sealed record TemplatesSelectionScope
        {
            public Guid? BranchId { get; private init; }

            public static TemplatesSelectionScope ForBranch(Guid branchId)
            {
                return new TemplatesSelectionScope
                {
                    BranchId = branchId
                };
            }

            public static TemplatesSelectionScope ForAllBranches()
            {
                return new TemplatesSelectionScope
                {
                    BranchId = null
                };
            }
        }
    }
}
