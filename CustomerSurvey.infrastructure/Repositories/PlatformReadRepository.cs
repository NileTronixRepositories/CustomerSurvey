using BuildingBlock.Application.Abstraction.Persistence;
using CustomerSurvey.Application.Abstraction.Presistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.infrastructure.Repositories
{
    internal sealed class PlatformReadRepository<TEntity>
       : BuildingBlock.Infrastracture.Repositories.EfReadRepository<TEntity, PlatformReadMarker>,
         IReadRepository<TEntity>
       where TEntity : class
    {
        public PlatformReadRepository(IDbContextResolver<PlatformReadMarker> resolver)
            : base(resolver) { }
    }

    internal sealed class PlatformWriteRepository<TEntity>
        : BuildingBlock.Infrastracture.Repositories.EfWriteRepository<TEntity, PlatformWriteMarker>,
          IWriteRepository<TEntity>
        where TEntity : class
    {
        public PlatformWriteRepository(IDbContextResolver<PlatformWriteMarker> resolver)
            : base(resolver) { }
    }

    internal sealed class PlatformUnitOfWork
        : BuildingBlock.Infrastracture.Repositories.EfUnitOfWork<PlatformWriteMarker>,
          IUnitOfWork
    {
        public PlatformUnitOfWork(IDbContextResolver<PlatformWriteMarker> resolver)
            : base(resolver) { }
    }
}