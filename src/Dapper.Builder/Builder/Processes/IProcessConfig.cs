﻿using System;

namespace Dapper.Builder.Processes
{
    /// <summary>
    /// Process Configuration
    /// </summary>
    public interface IProcessConfig
    {

        /// <summary>
        /// Excludes a specific pipe from the process 
        /// </summary>
        /// <param name="type"></param>
        void Exclude(params Type[] type);
    }
}