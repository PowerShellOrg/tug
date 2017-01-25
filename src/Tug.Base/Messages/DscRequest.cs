/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Tug.Messages
{
    /// <summary>
    /// Base class for DSC request input model defining common input
    /// elements found in the request URL or from request headers for
    /// any request revolves around a specific Agent.
    /// </summary>

    public abstract class DscRequest
    {
        public const string PROTOCOL_VERSION_HEADER = "ProtocolVersion";

        public const string X_MS_DATE_HEADER = "x-ms-date";

        // e.g. x-ms-date: 2016-08-15T21:25:51.8654321Z
        // Should be applied to a UTC time, based on the "Round-trip date/time pattern"
        // format specifier ("O") as defined here:
        //    https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx#Roundtrip
        public const string X_MS_DATE_FORMAT = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'";

        [FromHeader(Name = "Content-type")]
        public string ContentTypeHeader
        { get; set; }

        [FromHeader(Name = "Accept")]
        public string AcceptHeader
        { get; set; }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt590240.aspx
        /// </summary>
        [FromHeader(Name = "Authorization")]
        public string AuthorizationHeader
        { get; set; }

        [FromHeader(Name = X_MS_DATE_HEADER)]
        public string MsDateHeader
        { get; set; }

        [FromHeader(Name = PROTOCOL_VERSION_HEADER)]
        [Required]
        public string ProtocolVersionHeader
        { get; set; }

        /// <summary>
        /// Returns the Agent ID passed in the request message, however it
        /// may have been conveyed.  If the request does not receive an Agent ID
        /// then returns <c>null</c>.
        /// </summary>
        /// <remarks>
        /// Unlike most of the other fields the Agent ID may be conveyed as part
        /// of the route or an HTTP request header field.  This method allows a
        /// caller to consistently retrieve the ID regardless.
        /// </remarks>
        public virtual Guid? GetAgentId() => null;

        /// <summary>
        /// Returns the body content object captured in the request message. If
        /// the request does not capture an object representing the body content
        /// then returns <c>null</c>.
        /// </summary>
        public virtual object GetBody() => null;
    }

    /// <summary>
    /// Base class for DSC request input model defining common input
    /// elements found in the request URL or from request headers for
    /// any request revolves around a specific Agent.
    /// </summary>
    public abstract class DscAgentRequest : DscRequest
    {
        [FromRoute]
        [Required]
        public Guid? AgentId
        { get; set; }

        [FromRoute]
        public Guid ConfigurationId
        { get; set; }

        /// <summary>
        /// A version string represented as either an empty string representing
        /// no value or a 2-part or 4-part numeric specification separated by
        /// dots, e.g. <c>1.2</c> or <c>1.2.3.4</c>.
        [FromRoute]
        [RegularExpression("(\\d+\\.\\d+(\\.\\d+\\.\\d+)?)?")]
        public string ModuleVersion
        { get; set; }

        public override Guid? GetAgentId() => AgentId;
    }
}