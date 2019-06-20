
namespace Dapper.Builder.Services
{
    /// <summary>
    /// Service that creates joins strings
    /// </summary>
    public interface IJoinHandler
    {
        /// <summary>
        /// Produces a join string
        /// </summary>
        /// <param name="property">The Table name that would be joined on</param>
        /// <param name="condition">The string of the condition</param>
        /// <param name="type">Type of Join</param>
        /// <returns></returns>
        JoinQuery Produce(string property, string condition, JoinType type);
    }
}