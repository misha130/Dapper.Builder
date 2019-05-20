using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Builder.Services.DAL.Builder.JoinHandler
{
    public class JoinHandler : IJoinHandler
    {
        public JoinQuery Produce(string property, string condition, JoinType type)
        {
            return new JoinQuery
            {
                JoinType = JoinString(type),
                Table = property,
                Condition = condition,
            };
        }

        private string JoinString(JoinType type)
        {
            switch (type)
            {
                case JoinType.Inner:
                    return "INNER JOIN";
                case JoinType.Full:
                    return "FULL OUTER JOIN";
                case JoinType.Left:
                    return "LEFT JOIN";
                case JoinType.Right:
                    return "RIGHT JOIN";
            }
            throw new ArgumentException("Unsupported join type");
        }
    }
}
