using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tug.Messages
{
    /// <summary>
    /// Base class for DSC request input model defining common input
    /// elements found in the request URL or from request headers for
    /// any request revolves around a specific Agent.
    /// </summary>

    public abstract class DscRequest
    {

        [FromHeader(Name = "Content-type")]
        public string ContentTypeHeader
        { get; set; }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt590240.aspx
        /// </summary>
        [FromHeader(Name = "Authorization")]
        public string AuthorizationHeader
        { get; set; }

        [FromHeader(Name = "x-ms-date")]
        public string MsDateHeader
        { get; set; }

        [Required]
        [FromHeader(Name = "ProtocolVersion")]
        public string ProtocolVersionHeader
        { get; set; }
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
        public Guid AgentId
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
    }
}