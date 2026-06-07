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

namespace CustomerSurvey.Application.Features.Templates.Command.RestoreTemplate
{
    internal sealed class RestoreTemplateCommandHandler
       : ICommandHandler<RestoreTemplateCommand, RestoreTemplateResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteRepository<Template> _templateWriteRepository;
        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;
        private readonly ICurrentBranchScopeResolver _currentBranchScopeResolver;
        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public RestoreTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
            ICurrentBranchScopeResolver currentBranchScopeResolver,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork)
        {
            _templateReadRepository = templateReadRepository
                ?? throw new ArgumentNullException(nameof(templateReadRepository));

            _templateWriteRepository = templateWriteRepository
                ?? throw new ArgumentNullException(nameof(templateWriteRepository));

            _branchAdminReadRepository = branchAdminReadRepository
                ?? throw new ArgumentNullException(nameof(branchAdminReadRepository));

            _branchUserReadRepository = branchUserReadRepository
                ?? throw new ArgumentNullException(nameof(branchUserReadRepository));

            _currentBranchScopeResolver = currentBranchScopeResolver
                ?? throw new ArgumentNullException(nameof(currentBranchScopeResolver));

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<RestoreTemplateResponse>> Handle(
            RestoreTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<RestoreTemplateResponse>.Fail(new Error(
                    Code: "Templates.Restore.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            var currentBranchScope = await _currentBranchScopeResolver.ResolveAsync(
                cancellationToken);

            if (currentBranchScope.IsFailure)
            {
                return Result<RestoreTemplateResponse>.Fail(currentBranchScope.Errors);
            }

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForRestoreSpec(
                    templateId: request.TemplateId,
                    branchId: currentBranchScope.Value.BranchId),
                cancellationToken);

            if (template is null)
            {
                return Result<RestoreTemplateResponse>.Fail(new Error(
                    Code: "Templates.Restore.TemplateNotFound",
                    Message: ErrorMessage.RestoreTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (template.IsActive)
            {
                return Result<RestoreTemplateResponse>.Fail(new Error(
                    Code: "Templates.Restore.TemplateAlreadyActive",
                    Message: ErrorMessage.RestoreTemplate_Template_AlreadyActive,
                    Type: ErrorType.Validation));
            }

            template.Restore();

            _templateWriteRepository.Update(template);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new RestoreTemplateResponse
            {
                TemplateId = template.Id,
                BranchId = template.BranchId,
                NameEn = template.NameEn,
                NameAr = template.NameAr,
                Description = template.Description,
                Status = template.Status.ToString(),
                IsActive = template.IsActive
            };

            return Result<RestoreTemplateResponse>.Ok(response);
        }
    }
}
