/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;

namespace Tug.Server.Configuration
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