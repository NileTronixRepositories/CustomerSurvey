using BuildingBlock.Application.Abstraction;
using BuildingBlock.Application.Abstraction.Security;
using BuildingBlock.Domain.Results;
using CustomerSurvey.Application.Abstraction.Presistence;
using CustomerSurvey.Domain.Identity;
using CustomerSurvey.Domain.Resources;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemSurveyResponseDetails;

internal sealed class GetSystemSurveyResponseDetailsQueryHandler(
    IWriteReadRepository<SuperAdmin> superAdmins,
    AuthorizedResponseDetailsReader responseReader,
    ICurrentUser currentUser)
    : IQueryHandler<GetSystemSurveyResponseDetailsQuery, GetSystemSurveyResponseDetailsResponse>
{
    public async Task<Result<GetSystemSurveyResponseDetailsResponse>> Handle(
        GetSystemSurveyResponseDetailsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
        {
            return Result<GetSystemSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.SystemResponseDetails.Unauthenticated",
                Message: ErrorMessage.Auth_Token_Missing,
                Type: ErrorType.Security));
        }

        var currentSuperAdminExists = await superAdmins.AnyAsync(
            x => x.ApplicationUserId == currentUser.UserId.Value,
            cancellationToken);

        if (!currentSuperAdminExists)
        {
            return Result<GetSystemSurveyResponseDetailsResponse>.Fail(new Error(
                Code: "Reports.SystemResponseDetails.CurrentSuperAdminNotFound",
                Message: ErrorMessage.GetSystemSurveyResponseDetails_CurrentSuperAdmin_NotFound,
                Type: ErrorType.NotFound));
        }

        return await responseReader.ReadAsync(request.SurveyResponseId, requireFlags: false, cancellationToken);
    }
}
