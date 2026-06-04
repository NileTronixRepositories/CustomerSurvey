using BuildingBlock.Domain.Results;

namespace CustomerSurvey.Application.Abstraction.Security
{
    public interface ICurrentBranchScopeResolver
    {
        Task<Result<CurrentBranchScope>> ResolveAsync(CancellationToken cancellationToken = default);
    }
}
