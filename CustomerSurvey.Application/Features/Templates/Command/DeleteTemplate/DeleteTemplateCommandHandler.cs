using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.DeleteTemplate
{
    internal sealed class DeleteTemplateCommandHandler
        : ICommandHandler<DeleteTemplateCommand, DeleteTemplateResponse>
    {
        private readonly IWriteReadRepository<Template> _templateReadRepository;
        private readonly IWriteRepository<Template> _templateWriteRepository;

        private readonly IWriteReadRepository<BranchAdmin> _branchAdminReadRepository;
        private readonly IWriteReadRepository<BranchUser> _branchUserReadRepository;

        private readonly ICurrentUser _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTemplateCommandHandler(
            IWriteReadRepository<Template> templateReadRepository,
            IWriteRepository<Template> templateWriteRepository,
            IWriteReadRepository<BranchAdmin> branchAdminReadRepository,
            IWriteReadRepository<BranchUser> branchUserReadRepository,
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

            _currentUser = currentUser
                ?? throw new ArgumentNullException(nameof(currentUser));

            _unitOfWork = unitOfWork
                ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Result<DeleteTemplateResponse>> Handle(
            DeleteTemplateCommand request,
            CancellationToken cancellationToken)
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            {
                return Result<DeleteTemplateResponse>.Fail(new Error(
                    Code: "Templates.Delete.Unauthenticated",
                    Message: ErrorMessage.Auth_Token_Missing,
                    Type: ErrorType.Security));
            }

            var currentApplicationUserId = _currentUser.UserId.Value;

            Guid? actorBranchId = null;

            var branchAdmin = await _branchAdminReadRepository.FirstOrDefaultAsync(
                new GetCurrentBranchAdminForDeleteTemplateSpec(currentApplicationUserId),
                cancellationToken);

            if (branchAdmin is not null)
            {
                actorBranchId = branchAdmin.BranchId;
            }
            else
            {
                var branchUser = await _branchUserReadRepository.FirstOrDefaultAsync(
                    new GetCurrentBranchUserForDeleteTemplateSpec(currentApplicationUserId),
                    cancellationToken);

                if (branchUser is not null)
                {
                    actorBranchId = branchUser.BranchId;
                }
            }

            if (!actorBranchId.HasValue)
            {
                return Result<DeleteTemplateResponse>.Fail(new Error(
                    Code: "Templates.Delete.CurrentBranchActorNotFound",
                    Message: ErrorMessage.DeleteTemplate_CurrentBranchActor_NotFound,
                    Type: ErrorType.Security));
            }

            var branchId = actorBranchId.Value;

            var template = await _templateReadRepository.FirstOrDefaultAsync(
                new GetTemplateForDeleteSpec(
                    templateId: request.TemplateId,
                    branchId: branchId),
                cancellationToken);

            if (template is null)
            {
                return Result<DeleteTemplateResponse>.Fail(new Error(
                    Code: "Templates.Delete.TemplateNotFound",
                    Message: ErrorMessage.DeleteTemplate_Template_NotFound,
                    Type: ErrorType.NotFound));
            }

            if (!template.IsActive)
            {
                return Result<DeleteTemplateResponse>.Fail(new Error(
                    Code: "Templates.Delete.TemplateAlreadyInactive",
                    Message: ErrorMessage.DeleteTemplate_Template_AlreadyInactive,
                    Type: ErrorType.Validation));
            }

            template.Deactivate();

            _templateWriteRepository.Update(template);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeleteTemplateResponse
            {
                TemplateId = template.Id,
                BranchId = template.BranchId,
                NameEn = template.NameEn,
                IsActive = template.IsActive
            };

            return Result<DeleteTemplateResponse>.Ok(response);
        }
    }
}