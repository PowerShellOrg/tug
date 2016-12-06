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
            Console.WriteLine("Tug.Server starting up...");

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
