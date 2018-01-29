using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TugDSC.Server.WebAppHost
{
    /// This helper class allows us to access the logging subsystem
    /// before runtime logging is fully configured.  It sets up some
    /// hard-coded defaults with support for some environment-based
    /// configuration to control its behavior.
    public static class StartupLogger
    {
        /// An optional environment variable that points to a full file
        /// path that we load to pass on to the Console logging provider.
        public const string STARTUP_LOG_CONFIG = "TUG_STARTUP_LOG_CONFIG";

        private static LoggerFactory _startupLoggerFactory;

        static StartupLogger()
        {
            var cfgFile = System.Environment.GetEnvironmentVariable(STARTUP_LOG_CONFIG);

            _startupLoggerFactory = new LoggerFactory();

            if (!string.IsNullOrEmpty(cfgFile))
            {
                var cfg = new ConfigurationBuilder()
                        .AddJsonFile(cfgFile, optional: false)
                        .Build();
                _startupLoggerFactory.AddConsole(cfg);
            }
            else
            {
                _startupLoggerFactory.AddConsole();
            }
        }

        public static ILogger CreateLogger(string logName) =>
                _startupLoggerFactory.CreateLogger(logName);

        public static ILogger<T> CreateLogger<T>() =>
                _startupLoggerFactory.CreateLogger<T>();
    }
}