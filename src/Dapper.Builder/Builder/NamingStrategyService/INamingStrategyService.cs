using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Dapper.Builder.Builder.NamingStrategyService
{
    public interface INamingStrategyService
    {
        string GetTableName(Type type);
        string GetTableName(string name);
        string GetColumnName<T>(string name, string alias = null);
    }
}
