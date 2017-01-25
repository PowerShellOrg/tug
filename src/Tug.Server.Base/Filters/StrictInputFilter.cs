using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Tug.Util;

namespace Tug.Server.Filters
{
    /// <summary>
    /// An <see cref="IActionFilter">Action Filter</see> that validates the request
    /// input content to make sure it strictly conforms to the associated model class.
    /// </summary>
    /// <remarks>
    /// This filter inspects each resolved action arugment and foreach one that
    /// defines member properties that implement the <see cref="IExtData">External
    /// Data</see> interface, it computes recursively if there are any external data
    /// values set.  If so, that means that there was input data that did not conform
    /// strictly to the associaated data model and therefore will result in a
    /// Bad Request (400) response, aborting any subsequent action invocation.
    /// </remarks>
    public class StrictInputFilter : IActionFilter
    {
        protected ILogger<StrictInputFilter> _logger;

        public StrictInputFilter(ILogger<StrictInputFilter> logger)
        {
            _logger = logger;
        }

        public virtual void OnActionExecuting(ActionExecutingContext context)
        {
            int extDataCount = 0;
            foreach (var arg in context.ActionArguments)
            {
                int argExtDataCount = GetExtDataCount(arg.Value);
                if (argExtDataCount > 0)
                {
                    _logger.LogWarning("Found action argument [{arg}] with [{argExtDataCount}] extra data elements",
                            arg.Key, argExtDataCount);
                }
                extDataCount += argExtDataCount;
            }

            if (extDataCount > 0)
            {
                context.Result = new BadRequestResult();
            }
        }

        public virtual void OnActionExecuted(ActionExecutedContext context)
        { }

        /// <summary>
        /// Recursively identifies any properties that implement the IExtData
        /// interface and comutes the total count of ext data elements.
        /// </summary>
        protected int GetExtDataCount(params object[] values)
        {
            var extDataCount = 0;
            var extDataProps = 0;
            if (values != null && values.Length > 0)
            {
                foreach (var value in values)
                {
                    var valueType = value.GetType();
                    foreach (var prop in valueType.GetTypeInfo().GetProperties())
                    {
                        if (typeof(IExtData).IsAssignableFrom(prop.PropertyType))
                        {
                            ++extDataProps;
                            var extData = (IExtData)prop.GetValue(value);
                            if (extData != null)
                            {
                                extDataCount += extData.GetExtDataCount();
                                extDataCount += GetExtDataCount(extData);
                            }
                        }
                        else if (typeof(IEnumerable<IExtData>).IsAssignableFrom(prop.PropertyType))
                        {
                            ++extDataProps;
                            var extDataCollection = (IEnumerable<IExtData>)prop.GetValue(value);
                            if (extDataCollection != null)
                            {
                                foreach (var item in extDataCollection)
                                {
                                    extDataCount += item.GetExtDataCount();
                                    extDataCount += GetExtDataCount(item);
                                }
                            }
                        }
                    }
                }
            }
            return extDataCount;
        }
    }
}