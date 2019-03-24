// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

namespace TugDSC.Model
{
    public static class CommonValues
    {
        public static readonly string[] EMPTY_STRINGS = new string[0];
    }

    /// <summary>
    /// Defines a collection of constants representing MIME content types
    /// that are in use by the DSCPM protocol specification.
    /// </summary>
    public static class DscContentTypes
    {
        public const string OCTET_STREAM = "application/octet-stream";
        public const string JSON = "application/json";
    }
    
    /// <summary>
    /// An enumeration that is commensurate with a boolean type.
    /// </summary>
    /// <remarks>
    /// In a few places in the DSCPM message specifications, an element
    /// takes on semantics of a boolean value, but instead of using a
    /// JSON boolean, the specification uses a string enumeration.
    /// </remarks>
    public enum DscTrueFalse
    {
        False,
        True,
    }

    /// <summary>
    /// An enumeration that defines the various operation modes
    /// that are available for an LCM node. 
    /// </summary>
    public enum DscRefreshMode
    {
        Push,
        Pull,
    }

    /// <summary>
    /// An enumeration that defines the various statuses that indicate
    /// a node's disposition for needing to update its configuration.
    /// </summary>
    public enum DscActionStatus
    {
        OK,
        RETRY,
        GetConfiguration,
        UpdateMetaConfiguration,
    }

    public static class CommonRegistrationMessageTypes
    {
        public const string ConfigurationRepository = "ConfigurationRepository";
        public const string ResourceRepository = "ResourceRepository";
        public const string ReportServer = "ReportServer";
    }
}