using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace tug
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var routeBuilder = new RouteBuilder(app);

            // Node registration
            /*
                This is fired to each configured pull server and/or report server the first time
                a node tries to contact. See example Wireshark trace for reference. It is presently
                unknown what the Certificate information is to be used for. Also note that the LCM
                RegistrationKey information is not present; querying MS on that.

                Notice that the node can supply a list of all configuration names it is currently
                configured with. The pull server is expected to remember these.

                Example URL: Nodes(AgentId='91E51A37-B59F-11E5-9C04-14109FD663AE')
                Input example (JSON, in request body):
                    { "AgentInformation":
                        { 
                        "LCMVersion": "2.0",
                        "NodeName": "hostname",
                        "IPAddress": "ip_address(1)"
                        },
                        
                    "ConfigurationNames":
                        [ (array of strings) ],
                        
                    "RegistrationInformation":
                        {
                        "CertificateInformation":
                            {
                            "FriendlyName": "name",
                            "Issuer": "issuer",
                            "NotAfter": "date",
                            "NotBefore": "date",
                            "Subject": "subject",
                            "PublicKey": "key",
                            "Thumbprint": "thumbprint",
                            "Version": "int"
                            },
                        "RegistrationMessageType": "ConfigurationRepository(2)"
                        }
                    }

                Notes:
                (1) Semicolon-delimited list of IP addresses, including IPv4 and IPv6, in a single string
                (2) Will be "ReportServer" for a reporting server registration
            */ 
            // TODO: This needs to be a PUT, not a POST
            routeBuilder.MapPut("Nodes(AgentId={AgentId})", context =>
                {
                    var AgentId = context.GetRouteData().Values["AgentId"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"Registering node {AgentId}");
                }
            );

            // DSC Action
            /*
                This is sent to the pull server on each node consistency check. The node is basically saying,
                "for this configuration, here is my current checksum. Do I have the latest or not?" Notice that this example
                is for a node with only one configuration; the pull server is expected to know what that is, and simply
                comapre the provided checksum to its own checksum of the MOF file.

                Example URL: Nodes(AgentId='91E51A37-B59F-11E5-9C04-14109FD663AE')/GetDscAction
                Input example (JSON, in request body):
                    {
                        "ClientStatus": [
                            {
                                "Checksum": "checksum",
                                "ChecksumAlgorithm": "SHA-256"
                            }
                        ]
                    }
                
                Server is expected to return a 404 if the node is not registered.

                Server is expected to return a 200 otherwise, and include the following JSON in the response body:
                    {
                        "odata.metadata": "http://server-address:port/api-endpoint/$metadata#MSFT.DSCNodeStatus",
                        "NodeStatus": "Ok",
                        "Details": {
                            [
                                {
                                    "ConfigurationName": "config-name",
                                    "Status": "Ok"
                                }
                            ]
                        }
                    }

                Valid server responses are "GetConfiguration," meaning the node should retrieve a more current version
                of the configuration, and "Ok," meaning the node has the current version of the configuration, based
                upon the checksum. The checksum is simply a SHA-256 checksum of the MOF file containing the configuration
                (e.g., New-DscChecksum command). Notice that GetConfiguration would appear in two locations in the above
                example, should the node in fact need to re-get its configuration.

                For nodes with partial configurations, the JSON request is somewhat different.
                {
                    "ClientStatus": [
                        {
                            "Checksum": "checksum",
                            "ConfigurationName": "name",
                            "ChecksumAlgorithm": "SHA-256"
                        },
                        {
                            "Checksum": "checksum",
                            "ConfigurationName": "name",
                            "ChecksumAlgorithm": "SHA-256"
                        }
                    ]
                }

                The server response, however, can be for the entire configuration, and appear as the above. Do not
                rely on the Details[] array being complete, however. If the overall NodeStatus is not Ok, you should
                retrieve configurations.
            */
            routeBuilder.MapPost("Nodes(AgentId={AgentId})/DscAction", context =>
                {
                    var AgentId = context.GetRouteData().Values["AgentId"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"DSC action for node {AgentId}");
                }
            );

            // Asking for a MOF
            routeBuilder.MapPost("Nodes(AgentId={AgentId})/Configurations(ConfigurationName={ConfigurationName})/ConfigurationContent", context =>
                {
                    var AgentId = context.GetRouteData().Values["AgentId"];
                    var ConfigurationName = context.GetRouteData().Values["ConfigurationName"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"Request from node {AgentId} for configuration {ConfigurationName}");
                }
            );

            // Asking for a module
            routeBuilder.MapPost("Modules(ModuleName={ModuleName},ModuleVersion={ModuleVersion})/ModuleContent", context =>
                {
                    var ModuleName = context.GetRouteData().Values["ModuleName"];
                    var ModuleVersion = context.GetRouteData().Values["ModuleVersion"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"Module request for {ModuleName} version {ModuleVersion}");
                }
            );

            // Sending a report
            routeBuilder.MapPost("Nodes(AgentId={AgentId})/SendReport", context =>
                {
                    var AgentId = context.GetRouteData().Values["AgentId"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"Report from node {AgentId}");
                }
            );


            var routes = routeBuilder.Build();
            app.UseRouter(routes);

        }
    }
}
