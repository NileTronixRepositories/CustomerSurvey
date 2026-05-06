using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Presistence
{
    public interface IReadRepository<TEntity>
    : BuildingBlock.Application.Repositories.IReadRepository<TEntity, PlatformReadMarker>
    where TEntity : class
    {
    }
}