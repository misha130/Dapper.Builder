using System;

namespace Dapper.Builder.Builder.Processes.Configuration
{
    public interface IProcessConfig
    {
        void Exclude(params Type[] type);
    }
}