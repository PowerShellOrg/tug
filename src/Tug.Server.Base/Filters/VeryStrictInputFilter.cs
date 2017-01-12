using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tug.Messages;

namespace Tug.Server.Filters
{
    /// <summary>
    /// An <see cref="IActionFilter">Action Filter</see> that validates the request
    /// input content to make sure it (very) strictly conforms to the associated model class.
    /// </summary>
    /// <remarks>
    /// This filter builds upon the <see cref="StrictInputFilter"/> filter and adds
    /// additional strict validation checks such as making sure that each JSON body
    /// payload is serialized and deserialized in an exact and predictable form
    /// as dictated by action's associated DSC Request message model class.  This
    /// would include the exact order and element type of each JSON properties.
    /// </remarks>
    public class VeryStrictInputFilter : StrictInputFilter, IAuthorizationFilter
    {
        public VeryStrictInputFilter(ILogger<StrictInputFilter> logger)
            : base(logger)
        { }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var body = context?.HttpContext?.Request?.Body;

            if (body != null)
            {
                using (var ms = new MemoryStream())
                {
                    body.CopyTo(ms);
                    var bodyBytes = ms.ToArray();
                    context.HttpContext.Items["bodyBytes"] = bodyBytes;
                    context.HttpContext.Request.Body = new MemoryStream(bodyBytes);
                }
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var input = (context.ActionArguments?.FirstOrDefault())?.Value;
            var dscRequ = input as DscRequest;
            var bodyBytes = context.HttpContext.Items["bodyBytes"] as byte[];
            var bodyBytesLen = (int)bodyBytes?.Length;

            if (dscRequ != null)
            {
                var dscRequBody = dscRequ.GetBody();
                if (dscRequBody == null && bodyBytesLen == 0)
                {
                    // No body found and no body expected, we're good
                    if (_logger.IsEnabled(LogLevel.Trace))
                        _logger.LogTrace("No body content received and none expected in DSC Request");
                }
                else if (bodyBytesLen == 0)
                {
                    _logger.LogWarning("Expected DSC Request input body, but found nothing");
                    context.Result = new BadRequestResult();
                    return;
                }
                else if (dscRequBody == null)
                {
                    _logger.LogWarning("Unexpected input body content found");
                    context.Result = new BadRequestResult();
                }
                else
                {
                    var bodyDeser = Encoding.UTF8.GetString(bodyBytes);
                    var inputSer = JsonConvert.SerializeObject(dscRequBody);
                    if (bodyDeser != inputSer)
                    {
                        _logger.LogWarning("Re-serialized representation does not match actual input");
                        context.Result = new BadRequestResult();
                    }
                }
            }
        }
    }
}