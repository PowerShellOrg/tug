using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Tug.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Print some startup diagnostic information
            Console.WriteLine($"Tug.Server START-UP:");
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
