using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Application.Abstraction.Persistence
{
    public interface IDbContextResolver<in TMarker>
    {
        DbContext Resolve();
    }
}