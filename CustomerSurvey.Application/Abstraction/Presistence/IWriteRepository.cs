using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Presistence
{
    public interface IWriteRepository<TEntity>
        : BuildingBlock.Application.Repositories.IWriteRepository<TEntity, PlatformWriteMarker>
        where TEntity : class
    {
    }
}