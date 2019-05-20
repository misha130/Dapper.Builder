using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder.JoinHandler
{
    public interface IJoinHandler
    {
        JoinQuery Produce(string property, string condition, JoinType type);
    }
}
