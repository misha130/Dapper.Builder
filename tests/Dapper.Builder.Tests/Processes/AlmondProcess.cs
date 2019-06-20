
using Dapper.Builder.Processes;

namespace Dapper.Builder.Tests.Processes
{
    public class AlmondProcess : ISelectPipe, IInsertProcess, IUpdateProcess
    {
        public static bool AlmondsActive = false;
        public static bool AlmondPipeActive = false;
        public void Pipe<TEntity>(IQueryBuilder<TEntity> builder) where TEntity : new()
        {
            AlmondPipeActive = true;
        }

        public void Process<TEntity>(TEntity entity) where TEntity : new()
        {
            AlmondsActive = true;
        }
    }
}