using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Abstraction.Presistence
{
    public interface IUnitOfWork
    : BuildingBlock.Application.Repositories.IUnitOfWork<PlatformWriteMarker>
    {
    }
}