using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class JsonQueryFixtureBase : SharedStoreFixtureBase<NullSemanticsContext>, IQueryFixtureBase
    {
    }
}
