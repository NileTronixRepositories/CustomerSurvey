using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Presistence
{
    public interface IWriteReadRepository<TEntity>
     : global::BuildingBlock.Application.Repositories.IReadRepository<TEntity, PlatformWriteReadMarker>
     where TEntity : class
    {
    }
}