using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CustomerSurvey.infrastructure.Persistence
{
    internal sealed class PlatformWriteDbContextAccessor : IWriteDbContextAccessor
    {
        private readonly PlatformWriteDbContext _db;

        public PlatformWriteDbContextAccessor(PlatformWriteDbContext db) => _db = db;

        public DbContext GetDbContext() => _db;
    }
}