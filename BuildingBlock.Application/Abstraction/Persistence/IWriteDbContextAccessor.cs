using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Application.Abstraction.Persistence
{
    public interface IWriteDbContextAccessor
    {
        DbContext GetDbContext();
    }
}