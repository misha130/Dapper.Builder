using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder.PropertyParser {
    public interface IPropertyParser {
        IEnumerable<string> Parse<T> (Expression<Func<T, object>> expression, bool validate = true) where T : new ();

    }
}