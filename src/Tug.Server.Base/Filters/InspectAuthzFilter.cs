using System.IO;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Filters
{
    /// <summary>
    /// An authorization filter that's used to test and inspect the various elements
    /// of the filter/request context -- this is not meant to be used in a production
    /// capacity and offers no real functionality other than writing details to the console.
    /// </summary>
    public class InspectAuthzFilter : IAuthorizationFilter
    {
        private ILogger _logger;

        public InspectAuthzFilter(ILogger<InspectAuthzFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var cad = context.ActionDescriptor as ControllerActionDescriptor;
            _logger.LogInformation($"  Action[{context.ActionDescriptor.Id}] = [{context.ActionDescriptor.DisplayName}]");
            _logger.LogInformation($"  Route[{context.ActionDescriptor.AttributeRouteInfo.Name}]");

            System.Console.WriteLine($"  Action.......[{context.ActionDescriptor.Id}] = [{context.ActionDescriptor.DisplayName}]");
            System.Console.WriteLine($"  ActionName...[{cad?.ActionName}]");
            System.Console.WriteLine($"  RouteName....[{context.ActionDescriptor.AttributeRouteInfo.Name}]");

            byte[] body;
            using (var ms = new MemoryStream())
            {
                context.HttpContext.Request.Body.CopyTo(ms);
                body = ms.ToArray();
            }

            if (body != null)
            {
                // We need to replace the body that we previously read so that
                // it can be processed by the action, i.e. bound to an input model
                context.HttpContext.Request.Body = new MemoryStream(body);
            }

            System.Console.WriteLine($"  Body.Length = {body.Length}");
        }
    }
}