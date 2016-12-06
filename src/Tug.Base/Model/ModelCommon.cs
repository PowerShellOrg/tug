/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Model
{
    /// <summary>
    /// Defines a collection of constants representing MIME content types
    /// that are in use by the DSCPS protocol specification.
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
    /// In a few places in the DSCPS message specifications, an element
    /// takes on semantics of a boolean value, but instead of using a
    /// JSON boolean, the specification uses a string enumeration.
    /// </remarks>
    public enum DscTrueFalse
    {
        True,
        False,
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
}