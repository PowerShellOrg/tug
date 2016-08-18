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

            // Node/server communications are documented in a chapter excerpt located in the
            // References folder.

            // Node registration
            routeBuilder.MapPut("Nodes(AgentId={AgentId})", context =>
                {
                    var AgentId = context.GetRouteData().Values["AgentId"];
                    var Body = context.Request.Body;
                    var Headers = context.Request.Headers;
                    return context.Response.WriteAsync($"Registering node {AgentId}");
                }
            );

            // DSC Action
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
