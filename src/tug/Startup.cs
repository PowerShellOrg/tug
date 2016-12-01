using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json.Converters;

namespace tug
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                //.AddRouting()
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    // This enables converting Enums to/from their string names instead
                    // of their numerical value, based on:
                    //    * https://www.exceptionnotfound.net/serializing-enumerations-in-asp-net-web-api/
                    //    * https://siderite.blogspot.com/2016/10/controlling-json-serialization-in-net.html
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            // TODO:  This is temporary, we'll setup logic to optionally
            // support multiple providers of checksum algorithms
            services.AddSingleton<IChecksumAlgorithmProvider,
                    Providers.Sha256ChecksumAlgorithmProvider>();

            // TODO:  This is where we'll put logic to resolve the
            // selected DSC Handler based on a configured provider
            services.AddSingleton<IDscHandlerProvider,
                    Providers.BasicDscHandlerProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            // get configuration
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();

            // set up console logging
            // TODO it would be nice to also have a text file logger
            if (config["LogType"] == "console" | config["LogType"] == "both") {
                if (config["DebugLog"] == "true") {
                    loggerFactory.AddConsole(LogLevel.Debug);
                } else {
                    loggerFactory.AddConsole(LogLevel.Information);
                }
            }

            // initiate logging
            var logger = loggerFactory.CreateLogger("Tug");
            logger.LogInformation("Tug begins.");

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
<h1>Welcome to Tug!</h1>
<li><a href=""/version"">Version Info</a></li>
");
                });

                // Server version info
                routeBuilder.MapGet("version", context =>
                {
                    var version = GetType().GetTypeInfo().Assembly.GetName().Version;
                    return context.Response.WriteAsync($"{{{version}}}");
                });
            });

            // // Node registration
            // routeBuilder.MapPut("Nodes(AgentId={AgentId})", context =>
            //     {
            //         /*

            //             Authorization HTTP header must match the HTTP body,
            //             passed through a SHA-256 digest hash, 
            //             encoded in Base64.
            //             That then gets a newline, 
            //             the x-ms-date HTTP header from the request, 
            //             and is then run through a 
            //             SHA-256 digest hash that uses a known RegistrationKey as an HMAC, 
            //             with the result Base64 encoded.

            //             This essentially is a digital signature and proof that the node knows a shared secret registration key.

            //             So the received header is Authorization: Shared xxxxxxx\r\n

            //         */
            //         logger.LogInformation("\n\n\nPUT: Node registration");
            //         string AgentId = context.GetRouteData().Values["AgentId"].ToString();
            //         string Body = new StreamReader(context.Request.Body).ReadToEnd();
            //         var Headers = context.Request.Headers;
            //         logger.LogDebug("AgentId {AgentId}, Request Body {Body}, Headers {Headers}",AgentId,Body,Headers);

            //         // get needed headers
            //         // TODO: should check for these existing rather than assuming, fail if they don't
            //         string xmsdate = context.Request.Headers["x-ms-date"];
            //         string authorization = context.Request.Headers["Authorization"];
            //         logger.LogDebug("x-ms-date {date}, Authorization {auth}",xmsdate,authorization);

            //         // create signature, part 1
            //         // this is the request body, hashed, then combined with the x-ms-date header
            //         string contentHash = "";
            //         using(var sha256 = SHA256.Create()) {
            //             var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(Body));
            //             contentHash = Convert.ToBase64String(hashedBytes);
            //             //contentHash = BitConverter.ToString(hashedBytes).Replace("-","");
            //         }
            //         logger.LogDebug("Created content hash {hash}",contentHash);
            //         string stringToSign = String.Format("{0}\n{1}", contentHash, xmsdate);
            //         logger.LogDebug("String to sign is {sign}",stringToSign);

            //         // HACK - we need to run a command to get the allowed registration keys
            //         // and then compare each one
            //         string[] registrationKeys = {"91E51A37-B59F-11E5-9C04-14109FD663AE"};
            //         string result = Runner.Run("Get-Process");
            //         logger.LogDebug(result);

            //         // go through valid registration keys and create a final signature
            //         // we do this because we might have multiple registration keys, so we
            //         // have to try them all until we find one that matches
            //         bool Valid = false;
            //         foreach (string key in registrationKeys) {
            //             logger.LogDebug("Trying registration key {key}",key);

            //             // convert string key to Base64
            //             byte[] byt = Encoding.UTF8.GetBytes(key);
            //             string base64key = Convert.ToBase64String(byt);
    
            //             // create HMAC signature using this registration key
            //             var secretKeyBase64ByteArray = Convert.FromBase64String(base64key);
            //             string signature = "";
            //             using ( HMACSHA256 hmac = new HMACSHA256(secretKeyBase64ByteArray)) {
            //                 byte[] authenticationKeyBytes = Encoding.UTF8.GetBytes(stringToSign);
            //                 byte[] authenticationHash = hmac.ComputeHash(authenticationKeyBytes);
            //                 signature = Convert.ToBase64String(authenticationHash);
            //             }

            //             // compare what node sent to what we made
            //             string AuthToMatch = authorization.Replace("Shared ","");
            //             logger.LogDebug("Comparing keys:\nRcvd {0} \nMade {1}", AuthToMatch, signature );
            //             if (AuthToMatch == signature) {
            //                 logger.LogDebug("Node is authorized");
            //                 Valid = true;
            //                 break;
            //             }
            //         }

            //         // Because this is a PUT, we're only expected to return an HTTP status code
            //         // TODO we also need to call Set-TugNodeRegistration if the node was valid
            //         if (Valid) {
            //             // TODO return HTTP 200
            //             return context.Response.WriteAsync($"Registering node {AgentId}");
            //         } else {
            //             // TODO return HTTP 404(? check spec)
            //             return context.Response.WriteAsync($"Registering node {AgentId}");
            //         }
            //     }
            // );

            // // with everything defined, kick it off
            // var routes = routeBuilder.Build();
            // app.UseRouter(routes);
        }
    }
}
