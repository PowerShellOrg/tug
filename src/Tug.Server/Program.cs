/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tug.Server
{
    public class Program
    {
        public static bool _dumpEnvironment = true;
        // Always points to the current logger for this class.
        // Upon construction, we initialize this to a temporary <i>pre-logger</i>
        // that is hard-coded to simply write to the console but eventually we replace
        // this with a logger that is manufactored according to configuration specs.
        protected static ILogger _logger;

        public static void Main(string[] args)
        {
            // Print some startup diagnostic information
            Console.WriteLine($"Tug.Server START-UP:");
            _logger = AppLog.CreatePreLogger<Program>();
            _logger.LogInformation("Commencing PRE-logging on startup");
#if DOTNET_FRAMEWORK
            Console.WriteLine($"  .NET Platform = [.NET Framework]");
#else
            Console.WriteLine($"  .NET Platform = [.NET Core]");
#endif            

            Console.WriteLine($"  * ........CWD = [{Directory.GetCurrentDirectory()}]");
#if DOTNET_FRAMEWORK
            Console.WriteLine($"  * ....CmdLine = [{System.Environment.CommandLine}]");
            Console.WriteLine($"  * ....Is64bit = [{System.Environment.Is64BitProcess}]");
            Console.WriteLine($"  * .....ClrVer = [{System.Environment.Version}]");
            Console.WriteLine($"  * ......OsVer = [{System.Environment.OSVersion}]");
            Console.WriteLine($"  * ...UserName = [{System.Environment.UserName}]");
            Console.WriteLine($"  * ...Hostname = [{System.Environment.MachineName}]");
#endif
            Console.WriteLine();

            // Export some "runtime" meta data about our server which
            // may be referenced by other parts of the system, such as
            // logger output file paths or config input file paths
            System.Environment.SetEnvironmentVariable("TUG_RT_STARTDIR", Directory.GetCurrentDirectory());
#if DOTNET_FRAMEWORK
            System.Environment.SetEnvironmentVariable("TUG_RT_DOTNETFW", "NETFRAMEWORK");
#else
            System.Environment.SetEnvironmentVariable("TUG_RT_DOTNETFW", "NETCORE");
#endif

            if (_dumpEnvironment)
            {
                // Useful for debugging and diagnostics
                Console.WriteLine($"  * Environment:");
                var envKeys = System.Environment.GetEnvironmentVariables()
                    .Keys.Cast<string>().OrderBy(x => x);
                foreach (var e in envKeys)
                {
                    Console.WriteLine($"    o [{e}]=[{System.Environment.GetEnvironmentVariable((string)e)}]");
                }
                Console.WriteLine();
            }


           var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables(prefix: "ASPNETCORE_")
                .Build();
            
            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel()
                .UseUrls("http://*:5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseLoggerFactory(AppLog.Factory)
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
