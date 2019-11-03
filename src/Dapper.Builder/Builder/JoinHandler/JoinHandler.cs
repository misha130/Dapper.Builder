using System;

namespace Dapper.Builder.Services {
    public class JoinHandler : IJoinHandler {
        public JoinQuery Produce (string property, string condition, JoinType type) {
            return new JoinQuery {
                JoinType = JoinString (type),
                    Table = property,
                    Condition = condition,
            };
        }

        /// <summary>
        /// Converts type to join string for their relative database
        /// </summary>
        /// <param name="type">Joiner type</param>
        /// <returns></returns>
        private string JoinString (JoinType type) {
            switch (type) {
                case JoinType.Inner:
                    return "INNER JOIN";
                case JoinType.Full:
                    return "FULL OUTER JOIN";
                case JoinType.Left:
                    return "LEFT JOIN";
                case JoinType.Right:
                    return "RIGHT JOIN";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported join type");
            }
        }
    }
}