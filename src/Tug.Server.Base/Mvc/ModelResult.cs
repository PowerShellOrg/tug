// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tug.Messages.ModelBinding;

namespace Tug.Server.Mvc
{
    public static class ModelResultExt
    {
//        private static readonly ILogger LOG = AppLog.Create(typeof(ModelResultExt));

        public const string CONTENT_TYPE_TEXT = "text/plain";
        public const string CONTENT_TYPE_JSON = "application/json";
        public const string CONTENT_TYPE_OCTET_STREAM = "application/octet-stream";

        /// <summary>
        /// Defines an extension method for MVC Controllers that supports returning
        /// an <see cref="IActionResult">Action Result</see> that is defined by
        /// a response model class. 
        /// </summary>
        /// <remarks>
        /// This analogous to the default model binding behavior that MVC defines
        /// for controllers during the request processing stage, but is the
        /// complementary behavior to support the response processing stage.
        /// <para>
        /// Just like default MVC model binding, this routine works in concert
        /// with an attribute-decorated model POCO.  At this time it has special
        /// support for, and understands the following attributes:
        /// <list type="bullet">
        /// <item><see cref="ToHeaderAttribute">ToHeader</see></item>
        /// <item><see cref="ToResultAttribute">ToResult</see></item>
        /// </list>
        /// </para>
        /// </remarks>
        [NonAction] // Not really necessary on an ext method, but in case we ever move it to a Controller or Controll base class
        public static IActionResult Model(this ControllerBase c, object model)
        {
            PropertyInfo toResultProperty = null; // Used to detect more than one result property
            IActionResult result = null;

            var props = model.GetType().GetTypeInfo().GetProperties();
            
            foreach (var p in props)
            {
                var toHeader = p.GetCustomAttribute(typeof(ToHeaderAttribute))
                        as ToHeaderAttribute;
                if (toHeader != null)
                {
                    var headerName = toHeader.Name;
                    if (string.IsNullOrEmpty(headerName))
                        headerName = p.Name;

//                    if (LOG.IsEnabled(LogLevel.Debug))
//                        LOG.LogDebug($"Adding Header[{headerName}] replace=[{toHeader.Replace}]");

                    // TODO:  Add support for string[]???
                    var headerValue = ConvertTo<string>(p.GetValue(model, null));
                    if (toHeader.Replace)
                        c.Response.Headers[headerName] = headerValue;
                    else
                        c.Response.Headers.Add(headerName, headerValue);

                    continue;
                }

                var toResult = p.GetCustomAttribute(typeof(ToResultAttribute))
                        as ToResultAttribute;

                if (toResult != null)
                {
                    if (toResultProperty != null)
                        throw new InvalidOperationException("multiple Result-mapping attributes found");

                    toResultProperty = p;
                    var toResultType = p.PropertyType;

                    if (typeof(IActionResult).IsAssignableFrom(toResultType))
                    {
                        result = (IActionResult)p.GetValue(model, null);
                        continue;
                    }

                    var contentType = toResult.ContentType;

                    if (toResultType == typeof(byte[]))
                    {
                        var resultValue = (byte[])p.GetValue(model, null);
                        result = new FileContentResult(resultValue,
                                contentType ?? CONTENT_TYPE_OCTET_STREAM);
                        continue;
                    }
                    
                    if (typeof(Stream).IsAssignableFrom(toResultType))
                    {
                        var resultValue = (Stream)p.GetValue(model, null);
                        result = new FileStreamResult(resultValue,
                                contentType ?? CONTENT_TYPE_OCTET_STREAM);
                        continue;
                    }
                    
                    if (typeof(FileInfo).IsAssignableFrom(toResultType))
                    {
                        var resultValue = (FileInfo)p.GetValue(model, null);
                        result = new PhysicalFileResult(resultValue.FullName,
                                contentType ?? CONTENT_TYPE_OCTET_STREAM);
                        continue;
                    }
                    
                    if (typeof(string) == toResultType)
                    {
                        var resultValue = (string)p.GetValue(model, null);
                        result = new ContentResult
                        {
                            Content = resultValue,
                            ContentType = contentType
                        };
                        continue;
                    }

                    var pValue = p.GetValue(model, null);
                    if (pValue != null)
                    {
                        result = new JsonResult(pValue);
                    }
                }
            }

            if (result == null)
                result = new OkResult();
            
            return result;
        }

        public static T ConvertTo<T>(object value)
        {
            var tc = TypeDescriptor.GetConverter(typeof(T));
            return (T)tc.ConvertFrom(value);
        }
    }
}