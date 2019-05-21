using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Builder.Builder
{
    public interface IInternalQueryBuilder
    {
        int GetParamCount();

        void ParamCount(int count);
    }
}
