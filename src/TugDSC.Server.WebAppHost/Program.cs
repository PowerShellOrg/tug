// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TugDSC.Server.WebAppHost
{
    /// <summary>
    /// Main entry point for ASP.NET Core MVC-based Tug DSC server.
    /// More details about ASP.NET Core hosting can be found here:
    ///    https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting
    /// </summary>
    /// <remarks>
    /// The entry point builds and configures a <see cref="IWebHost">Web Host</see> for the
    /// Tug DSC server.
    /// <para>
    /// There are two sets of configurations that drive behavior of the server.  The hosting
    /// configuration is used to setup the Web Host directly and also influences behavior before
    /// startup and during the early phase of startup.  The application configuration is used
    /// to drive the normal runtime behavior of the late phase of startup and runtime operation
    /// after startup.
    /// </para>
    /// <para>
    /// TODO: Provide more details about configuration
    /// </para>
    /// </remarks>
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

        public const string HOST_CONFIG_CLI_PREFIX = "/h:";

        public const string SKIP_BANNER_CLI_ARG = "--skip-banner";
        public const string SKIP_DIAGNOSTICS_CLI_ARG = "--skip-diag";
        public const string RUN_AS_SERVICE_CLI_ARG = "--service";

        #endregion -- Constants --

        #region -- Fields --
        
        // Always points to the current logger for this class.
        // Upon construction, we initialize this to a temporary <i>pre-logger</i>
        // that is hard-coded to simply write to the console but eventually we replace
        // this with a logger that is manufactured according to configuration specs.
        protected static ILogger _logger;

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

                ["applicationName"] = "TugDSC-Server-WebAppHost",
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

        public static bool SkipBanner
        { get; set; } = false;

        public static bool SkipDiagnostics
        { get; set; } = false;

        public static bool RunAsService
        { get; set; } = false;

        /// <summary>
        /// If true, will print to standard out all resolved environment
        /// variables upon startup.
        /// </summary>
        /// <returns></returns>
        public static bool DumpEnvironment
        { get; set; } = false;

        public static IConfiguration HostingConfig
        { get; private set; }

        protected static IWebHost WebHost
        { get; private set; }

        #endregion -- Properties --

        #region -- Constructors --

        static Program()
        {
            // Setup a pre-logger to have a place to write out diagnostics and errors until
            // we have a chance to properly setup the final runtime logging configuration
            _logger = StartupLogger.CreateLogger<Program>();
            _logger.LogInformation("********** Commencing STARTUP LOGGING **********");
        }

        #endregion -- Constructors --

        #region -- Methods --

        public static void Main(string[] args)
        {
            // This is ugly as hell but unfortunately, we could not find another
            // way to pass this along from here to other parts of the app via DI
            CommandLineArgs = args;

            // Parse out some quick CLI flags
            SkipBanner = CommandLineArgs.Contains(SKIP_BANNER_CLI_ARG);
            SkipDiagnostics = CommandLineArgs.Contains(SKIP_DIAGNOSTICS_CLI_ARG);
            RunAsService = CommandLineArgs.Contains(RUN_AS_SERVICE_CLI_ARG);

            PrintBanner();

            DumpDiagnostics();

            _logger.LogInformation("Resolving hosting configuration");
            HostingConfig = ResolveHostingConfig(args);

            _logger.LogInformation("Building Web Host");
            WebHost = BuildWebHost(HostingConfig);

            Run(WebHost);
        }        

        protected static void PrintBanner()
        {
            if (SkipBanner)
                return;

            var asm = typeof(Program).Assembly;
            var asmName = asm.GetName();
            var asmVers = asmName.Version;
            var asmInfo = FileVersionInfo.GetVersionInfo(asm.Location);

            // The copyright rune may not print so well on console
            var copyright = asmInfo.LegalCopyright?.Replace("©", "(C)"); 

            Console.WriteLine($"TugDSC Server WebAppHost v{asmVers} -- starting up");
          //Console.WriteLine(asmInfo.ProductName);
            Console.WriteLine(copyright);
            Console.WriteLine();
        }

        protected static void DumpDiagnostics()
        {
            if (SkipDiagnostics)
                return;

            Console.WriteLine();
            Console.WriteLine($"  .NET Platform = [{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}]");
            Console.WriteLine($"  * Runtime Dir = [{System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()}]");
            Console.WriteLine($"  * ........CWD = [{Directory.GetCurrentDirectory()}]");
            Console.WriteLine($"  * ....CmdLine = [{System.Environment.CommandLine}]");
            Console.WriteLine($"  * ....Is64bit = [{System.Environment.Is64BitProcess}]");
            Console.WriteLine($"  * .....ClrVer = [{System.Environment.Version}]");
            Console.WriteLine($"  * ......OsVer = [{System.Environment.OSVersion}]");
            Console.WriteLine($"  * ...UserName = [{System.Environment.UserName}]");
            Console.WriteLine($"  * ...Hostname = [{System.Environment.MachineName}]");
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

        protected static IConfiguration ResolveHostingConfig(string[] args)
        {
            // var config = new ConfigurationBuilder()
            //     .AddCommandLine(args)
            //     .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            //     .Build();

            var configArgs = args.Where(x => x.StartsWith(HOST_CONFIG_CLI_PREFIX))
                    .Select(x => x.Substring(HOST_CONFIG_CLI_PREFIX.Length))
                    .ToArray();

            var basePath = Directory.GetCurrentDirectory();
            if (RunAsService)
                basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var hostingConfigBuilder = new ConfigurationBuilder()
                    // Base path for any file-based config sources
                    .SetBasePath(basePath)
                    // Default to a set of pre-defined default settings
                    .AddInMemoryCollection(_hostingDefaultSettings)
                    // Allow to optionally override with a JSON file
                    .AddJsonFile(HOST_CONFIG_FILENAME, optional: true)
                    // Allow to optionally override with env vars
                    .AddEnvironmentVariables(prefix: HOST_CONFIG_ENV_PREFIX)
                    // Allow to optionally override with CLI args
                    .AddCommandLine(configArgs);

            return hostingConfigBuilder.Build();
        }

        protected static IWebHost BuildWebHost(IConfiguration hostingConfig)
        {
            var hostBuilder = new WebHostBuilder()
                .UseConfiguration(hostingConfig)
                .ConfigureLogging((builderContext, loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                })
                // Use Kestrel as a web server
                .UseKestrel()
                // this must come after UseConfiguration because it
                // overrides several settings such as port, base path
                // and useStartupErrors config settings
                .UseIISIntegration()
                .UseStartup<Startup>();
            
            return hostBuilder.Build();
        }

        protected static void Run(IWebHost webHost)
        {
#if DOTNET_FRAMEWORK
            if (RunAsService)
            {
                _logger.LogInformation("Running as Windows Service");
                TugWebHostService.RunAsService(webHost);
                return;
            }
#endif // DOTNET_FRAMEWORK

            if (RunAsService) throw new NotImplementedException(
                    "RunAsService is currently only implemented for .NET Framework on Windows");

            webHost.Run();
        }

        #endregion -- Methods --
    }

#if DOTNET_FRAMEWORK
    /// Implements support for running as WinService.
    ///
    /// More details can be found here:
    ///    https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/windows-service
    internal class TugWebHostService : Microsoft.AspNetCore.Hosting.WindowsServices.WebHostService
    {
        private ILogger _logger;

        public TugWebHostService(IWebHost host) : base(host)
        {
            var lf = (ILoggerFactory)host.Services.GetService(typeof(ILoggerFactory));
            _logger = lf.CreateLogger<TugWebHostService>();
            _logger.LogInformation("WebHostService created");
        }

        public static IWebHost RunAsService(IWebHost webHost)
        {
            Run(new TugWebHostService(webHost));
            return webHost;
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogInformation("Received 'START' request");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            base.OnStarted();
            _logger.LogInformation("Service started.");
        }
        
        protected override void OnStopping()
        {
            _logger.LogInformation("Received 'STOP' request");
            base.OnStopping();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            _logger.LogInformation("Service stopped.");
        }
    }
#endif // DOTNET_FRAMEWORK

}
