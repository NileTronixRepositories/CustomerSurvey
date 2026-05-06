using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using CustomerSurvey.Application.Abstraction.Presistence;

namespace CustomerSurvey.infrastructure.Persistence
{
    internal sealed class PlatformReadDbContextResolver : IDbContextResolver<PlatformReadMarker>
    {
        private readonly PlatformReadDbContext _db;

        public PlatformReadDbContextResolver(PlatformReadDbContext db) => _db = db;

        public DbContext Resolve() => _db;
    }

    internal sealed class PlatformWriteDbContextResolver : IDbContextResolver<PlatformWriteMarker>
    {
        private readonly PlatformWriteDbContext _db;

        public PlatformWriteDbContextResolver(PlatformWriteDbContext db) => _db = db;

        public DbContext Resolve() => _db;
    }
}