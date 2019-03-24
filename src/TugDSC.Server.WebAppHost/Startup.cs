// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
// using NLog.Extensions.Logging;
// using NLog.Web;
using TugDSC.Server.Configuration;
using TugDSC.Server.Filters;
using TugDSC.Server.Util;

namespace TugDSC.Server.WebAppHost
{
    public class Startup
    {
        #region -- Constants --

        /// <summary>
        /// File name of a required JSON file used to configure the server app.
        /// </summary>
        public const string APP_CONFIG_FILENAME = "appsettings.json";

        /// Defines an optional CLI parameter that can be used to override the
        /// default configuration file.  If specified, the path to the config
        /// file should be specified immediately after (i.e. no space) and
        /// although not strictly enforced, should be fully qualified with
        /// complete path.
        public const string APP_CONFIG_CLI_OVERRIDE = "--config=";

        /// <summary>
        /// File name of an optional JSON file used to override server app configuration.
        /// </summary>
        public const string APP_USER_CONFIG_FILENAME = "appsettings.user.json";
        /// Defines an optional CLI parameter that can be used to override the
        /// default user configuration file.  If specified, the path to the config
        /// file should be specified immediately after (i.e. no space) and
        /// although not strictly enforced, should be fully qualified with
        /// complete path.
        public const string APP_USER_CONFIG_CLI_OVERRIDE = "--userconfig=";

        /// <summary>
        /// Prefix used to identify environment variables that can override server app
        /// configuration.
        /// </summary>
        public const string APP_CONFIG_ENV_PREFIX = "TUG_CFG_";

        public const string APP_CONFIG_CLI_PREFIX = "/c:";

        #endregion -- Constants --

        #region -- Fields --

        protected ILogger<Startup> _logger;

        protected IConfiguration _config;

        #endregion -- Fields --

        #region -- Constructors --

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Start with a pre-logger till the final
            // logging config is finalized down below
            _logger = StartupLogger.CreateLogger<Startup>();

            // This is ugly as hell but unfortunately, we could not find another
            // way to pass this along from Program to other parts of the app via DI
            var args = Program.CommandLineArgs?.ToArray();

            _logger.LogInformation("Resolving final runtime configuration");
            _config = ResolveAppConfig(args);

            // TODO: We may need to adjust based on changes in .NET 2.0
            ConfigureLogging(env, loggerFactory);
        }

        #endregion -- Constructors --

        #region -- Methods --

        // This method gets called by the runtime. Use this
        // method to add services to the container.  For more
        // information on how to configure your application,
        // visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("Configuring services registry");

            // Enable and bind to strongly-typed configuration
            var appSettings = _config.GetSection(nameof(AppSettings));
            services.AddSingleton<IConfiguration>(appSettings);
            services.AddOptions();
            services.Configure<AppSettings>(appSettings);
            services.Configure<ChecksumSettings>(
                    appSettings.GetSection(nameof(AppSettings.Checksum)));
            services.Configure<AuthzSettings>(
                    appSettings.GetSection(nameof(AppSettings.Authz)));
            services.Configure<HandlerSettings>(
                    appSettings.GetSection(nameof(AppSettings.Handler)));

            // Register a single instance of each filter type we'll use down below
            services.AddSingleton<DscRegKeyAuthzFilter.IAuthzStorageHandler,
                    DscRegKeyAuthzFilter.LocalAuthzStorageHandler>();
            services.AddSingleton<DscRegKeyAuthzFilter>();
            services.AddSingleton<StrictInputFilter>();
            services.AddSingleton<VeryStrictInputFilter>();

            // Add MVC-supporting services
            _logger.LogInformation("Adding MVC services");
            services.AddMvc(options =>
            {
                // Add the filter by service type reference
                options.Filters.AddService(typeof(DscRegKeyAuthzFilter));
                options.Filters.AddService(typeof(VeryStrictInputFilter));
            }).AddJsonOptions(options =>
                {
                    // This enables converting Enums to/from their string names instead
                    // of their numerical value, based on:
                    //    * https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
                    //    * https://siderite.blogspot.com/2016/10/controlling-json-serialization-in-net.html
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            
            // Register the Provider Managers 
            services.AddSingleton<ChecksumAlgorithmManager>();
            services.AddSingleton<DscHandlerManager>();

            // Register the Helpers
            services.AddSingleton<ChecksumHelper>();
            services.AddSingleton<DscHandlerHelper>();
        }

        // This method gets called by the runtime. Use this
        // method to configure the HTTP request pipeline.
        public void Configure(IServiceProvider serviceProvider,
                IApplicationBuilder app, IHostingEnvironment env)
        {
            // set development option
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // // begin registering routes for incoming requests
            // var routeBuilder = new RouteBuilder(app);
            app.UseMvc(routeBuilder =>
            {

                // Default route welcome message
                routeBuilder.MapGet("", context =>
                {
                    return context.Response.WriteAsync(@"
<h1>Welcome to TugDSC!</h1>
<li><a href=""/version"">Version Info</a></li>
");
                });

                // Server version info
                routeBuilder.MapGet("version", context =>
                {
                    var version = GetType().GetTypeInfo().Assembly.GetName().Version;
                    return context.Response.WriteAsync($@"{{""version"":""{version}""}}");
                });
            });

            // Resolve some DI classes to make sure they're ready to go when needed and
            // forces any possible resolution errors to invoke earlier rather than later
            serviceProvider.GetRequiredService<ChecksumHelper>();
            serviceProvider.GetRequiredService<DscHandlerHelper>();
        }

        protected IConfiguration ResolveAppConfig(string[] args = null)
        {
            var basePath = Directory.GetCurrentDirectory();
            if (Program.RunAsService)
                basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            // Resolve the app config filenames
            var jsonFile = args?.FirstOrDefault(x => x.StartsWith(APP_CONFIG_CLI_OVERRIDE))
                    ?.Substring(APP_CONFIG_CLI_OVERRIDE.Length) ?? APP_CONFIG_FILENAME;
             _logger.LogInformation("Resolved app config file as [{0}]", jsonFile);
            var userFile = args?.FirstOrDefault(x => x.StartsWith(APP_USER_CONFIG_CLI_OVERRIDE))
                    ?.Substring(APP_USER_CONFIG_CLI_OVERRIDE.Length) ?? APP_USER_CONFIG_FILENAME;
             _logger.LogInformation("Resolved user-local app config file as [{0}]", userFile);

            if (args?.Length > 0)
            {
                var jsonFileOverride = args.FirstOrDefault(x => x.StartsWith(APP_CONFIG_CLI_OVERRIDE));
                var userFileOverride = args.FirstOrDefault(x => x.StartsWith("--userconfig="));

                jsonFile = args.FirstOrDefault(x => x.StartsWith(APP_CONFIG_CLI_OVERRIDE))?.Substring(APP_CONFIG_CLI_OVERRIDE.Length) ?? jsonFile;
            }

            // Resolve the runtime configuration settings
            var appConfigBuilder = new ConfigurationBuilder();
            // Base path for any file-based config sources
            appConfigBuilder.SetBasePath(basePath);
            // Default location for all configuration settings
            appConfigBuilder.AddJsonFile(jsonFile, optional: false);
            // Optional location for user-specific local overrides
            appConfigBuilder.AddJsonFile(userFile, optional: true);
            // Allows overriding any setting using envVars that being with TUG_CFG_
            appConfigBuilder.AddEnvironmentVariables(prefix: APP_CONFIG_ENV_PREFIX);
            // A good place to store secrets for dev/test
            appConfigBuilder.AddUserSecrets<Startup>();

            if (args != null)
            {
                var configArgs = args.Where(x => x.StartsWith(APP_CONFIG_CLI_PREFIX))
                        .Select(x => x.Substring(APP_CONFIG_CLI_PREFIX.Length)).ToArray();
                // Last but not least, allow overriding with CLI arguments
                appConfigBuilder.AddCommandLine(configArgs);
            }

            return appConfigBuilder.Build();
        }

        protected void ConfigureLogging(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var logSettings = _config
                ?.GetSection(nameof(LogSettings))
                ?.Get<LogSettings>();

            _logger.LogInformation("Applying logging configuration");

            if (logSettings != null)
            {
                if (logSettings.LogType.HasFlag(LogType.Console)) {
                    _logger.LogInformation("  * enabling Console Logging");
                    if (logSettings.DebugLog) {
                        loggerFactory.AddConsole(LogLevel.Debug);
                    } else {
                        loggerFactory.AddConsole(LogLevel.Information);
                    }
                }

                // TODO: Resolve which logger to use
                // if (logSettings.LogType.HasFlag(LogType.NLog)) {
                //     var configPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");
                //     _logger.LogInformation($"  * enabling NLog with config=[{configPath}]");
                //     loggerFactory.AddNLog();
                //     env.ConfigureNLog(configPath);
                // }
            }

            // TODO: We may need to adjust based on changes in .NET 2.0            

            // Initiate and switch to runtime logging
            _logger.LogInformation("Instantiating runtime logging");
            _logger.LogInformation("********** Ceasing STARTUP LOGGING **********");
            _logger.LogInformation("");


            _logger = loggerFactory.CreateLogger<Startup>();
            _logger.LogInformation("********** Commencing RUNTIME LOGGING **********");
        }

        #endregion -- Methods --
    }
}