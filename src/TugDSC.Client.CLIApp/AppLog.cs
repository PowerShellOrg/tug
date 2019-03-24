// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using Microsoft.Extensions.Logging;

namespace TugDSC.Client
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
        // private static LoggerFactory _preLoggerFactory;

        static AppLog()
        {
            // // We set this up to log any events that take place before the
            // // ultimate logging configuration is finalized and realized
            // _preLoggerFactory = new LoggerFactory();
            // // Here we configure the hard-coded settings of the pre-logger with
            // // anything we want before the runtime logging config is resolved
            // _preLoggerFactory.AddConsole();

            // This will be the final runtime logger factory
            Factory = new LoggerFactory();
        }

        // public static ILogger<T> CreatePreLogger<T>()
        // {
        //     return _preLoggerFactory.CreateLogger<T>();
        // }

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