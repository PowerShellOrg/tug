// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TugDSC.Messages.ModelBinding
{
    /// <summary>
    /// Specifies that a property of a model class should be bound to a response header,
    /// when used in concert with the <see cref="ModelResultExt#Model">Model</see>
    /// extension method for MVC Controllers.  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ToHeaderAttribute : Attribute, IModelNameProvider
    {
        public ToHeaderAttribute()
        { }

        public string Name { get; set; }

        public bool Replace
        { get; set; }
    }
    
    /// <summary>
    /// Specifies that a property of a model class should be bound to a response content
    /// body or action result, when used in concert with the <see
    /// cref="ModelResultExt#Model">Model</see> extension method for MVC Controllers.
    /// </summary>
    /// <remarks>
    /// The return type of the property on which this attribute is decorated will be
    /// inspected and will be used to determine the type of action result or content
    /// type that is generated.  The following result types are understood:
    /// <list type="bullet">
    /// <item><c>IActionResult</co></item>
    /// <item><c>byte[]</co></item>
    /// <item><c>Stream</co></item>
    /// <item><c>FileInfo</co></item>
    /// <item><c>string</co></item>
    /// </list>
    /// If the property type is not one of the special types listed, then it will
    /// treated as a model class that will be serialized and returned as JSON.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ToResultAttribute : Attribute/*, IBindingSourceMetadata*/
    {
        public ToResultAttribute()
        { }

        public string ContentType
        { get; set; }
    }
}