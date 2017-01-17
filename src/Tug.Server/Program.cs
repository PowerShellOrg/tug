/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Tug.Server
{
    public class Program
    {
        #region -- Constants --

        /// <summary>
        /// File name of an optional JSON file used to configure the Web Host.
        /// </summary>
        public const string HOST_CONFIG_FILENAME = "hosting.json";

        /// <summary>
        /// Prefix used to identify environment variables that can override Web Host
        /// configuration.
        /// </summary>
        public const string HOST_CONFIG_ENV_PREFIX = "TUG_HOST_";

        #endregion -- Constants --

        #region -- Fields --
        
        // Always points to the current logger for this class.
        // Upon construction, we initialize this to a temporary <i>pre-logger</i>
        // that is hard-coded to simply write to the console but eventually we replace
        // this with a logger that is manufactered according to configuration specs.
        protected static ILogger _logger;

        protected static IConfiguration _hostingConfig;

        // Defines the hard-coded default configuration settings for the WebHostBuilder
        // these can be overridden via env vars and CLI switches or by IIS Integration
        private static IDictionary<string, string> _hostingDefaultSettings =
            new Dictionary<string, string>
            {
                // These default settings can be overridden by using environment
                // variables prefixed with 'TUG-HOST_' or by specifing on the CLI
                // e.g. --urls "http://*:4321/"
                // The list of host settings can be found here:
                //    https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting#configuring-a-host

                ["applicationName"] = "Tug.Server",
                ["environment"] = "PRODUCTION",
                ["captureStartupErrors"] = false.ToString(),
                ["contentRoot"] = Directory.GetCurrentDirectory(),
                ["detailedErrors"] = false.ToString(),
                ["urls"] = "http://*:5000", // "http://localhost:5080;https://localhost:5443"

            };

        #endregion -- Fields --

        #region -- Properties --

        /// <summary>
        /// Provides app-wide access to the runtime CLI args.
        /// </summary>
        /// <remarks>
        /// This is an unfortunate kludge because we could not find a clean way to
        /// make this accessible to app components using the DI mechanism.
        /// </remarks>
        public static IEnumerable<string> CommandLineArgs
        { get; private set; }

        /// <summary>
        /// If true, will print to standard out all resolved environment
        /// variables upon startup.
        /// </summary>
        /// <returns></returns>
        public static bool DumpEnvironment
        { get; set; } = false;

        #endregion -- Properties --

        #region -- Constructors --

        static Program()
        {
            // Setup a pre-logger to have a place to write out diagnostics and errors until
            // we have a chance to properly setup the final runtime logging configuration
            _logger = AppLog.CreatePreLogger<Program>();
            _logger.LogInformation("Commencing PRE-logging on startup");
        }

        #endregion -- Constructors --

        #region -- Methods --

        public static void Main(string[] args)
        {
            Console.WriteLine($"Tug.Server START-UP:");

            // This is ugly as hell but unfortunately, we could not find another
            // way to pass this along from here to other parts of the app via DI
            CommandLineArgs = args;

            DumpDiagnotics();

            _logger.LogInformation("Resolving hosting configuration");
            ResolveHostingConfig(args);

            var hostBuilder = new WebHostBuilder()
                .UseConfiguration(_hostingConfig)
                // Register the logging factory to use for the app which
                // we setup elsewhere to account for non-DI scenarios
                .UseLoggerFactory(AppLog.Factory)
                .UseKestrel()
                // this must come after UserConfiguration because it
                // overrides several settings such as port, base path
                // and useStartupErrors config settings
                .UseIISIntegration()
                .UseStartup<Startup>();
            
            var host = hostBuilder.Build();
            host.Run();
        }

        protected static IConfiguration ResolveHostingConfig(string[] args)
        {
            // var config = new ConfigurationBuilder()
            //     .AddCommandLine(args)
            //     .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            //     .Build();

            var hostingConfigBuilder = new ConfigurationBuilder()
                    // Base path for any file-based config sources
                    .SetBasePath(Directory.GetCurrentDirectory())
                    // Default to a set of pre-defined default settings
                    .AddInMemoryCollection(_hostingDefaultSettings)
                    // Allow to optionally override with a JSON file
                    .AddJsonFile(HOST_CONFIG_FILENAME, optional: true)
                    // Allow to optionally override with env vars
                    .AddEnvironmentVariables(prefix: HOST_CONFIG_ENV_PREFIX)
                    // Allow to optionally override with CLI args
                    .AddCommandLine(args);

            _hostingConfig = hostingConfigBuilder.Build();

            return _hostingConfig;
        }

        protected static void DumpDiagnotics()
        {
            Console.WriteLine();
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

            if (DumpEnvironment)
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
        }

        #endregion -- Methods --
    }
}
