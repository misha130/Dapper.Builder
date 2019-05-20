using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder.FilterParser {
    public interface IFilterParser<T> where T : new () {
        QueryResult Parse (Expression<Func<T, bool>> expression, ref int count);
        QueryResult Parse (Expression<Func<T, object>> expression, ref int count);

        QueryResult Parse<U, W> (Expression<Func<U, W, bool>> expression, ref int count) where U : new () where W : new ();

        void SetAlias<T> (string alias);

        void SetParentAlias<T> (string alias);
    }
}