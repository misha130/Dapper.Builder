using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Builder.Builder.SortHandler
{
    public class SortColumn
    {
        public string Field { get; set; }
        public SortType Dir { get; set; }
    }

    public enum SortType
    {
        Asc,
        Desc
    }
}
