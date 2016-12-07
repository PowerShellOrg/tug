/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using Microsoft.Extensions.Logging;

namespace Tug.Server
{
    /// <summary>
    /// Implements a static access pattern for ILoggerFactory.
    /// </summary>
    /// <remarks>
    /// This pattern is based on <see
    /// cref="https://msdn.microsoft.com/en-us/magazine/mt694089.aspx?f=255&MSPPError=-2147217396"
    /// >this static <c>ApplicationLogging</c> class</see> approach.
    /// This allows us to introduce logging to static classes (such as Extension
    /// Method classes) that cannot participate in dependency-injected services.
    /// </remarks>
    public static class AppLog
    {
        static AppLog()
        {
            Factory = new LoggerFactory();
        }

        public static ILoggerFactory Factory
        { get; }

        public static ILogger Create(Type t)
        {
            return Factory.CreateLogger(t);
        }

        public static ILogger<T> Create<T>()
        {
            return Factory.CreateLogger<T>();
        }
    }
}