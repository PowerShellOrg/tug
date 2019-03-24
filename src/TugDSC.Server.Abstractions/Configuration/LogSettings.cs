// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;

namespace TugDSC.Server.Configuration
{
    public class LogSettings
    {
        public LogType LogType
        { get; set; }

        public bool DebugLog
        { get; set; }
    }

    [Flags]
    public enum LogType
    {
        None = 0x0,

        Console = 0x1,

        NLog = 0x2,

        All = Console | NLog,
    }
}