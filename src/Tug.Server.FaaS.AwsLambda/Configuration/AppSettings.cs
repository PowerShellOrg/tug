/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Server.FaaS.AwsLambda.Configuration
{
    /// <summary>
    /// Defines configuration settings that drive the operational
    /// runtime behavior of the system.
    /// </summary>
    /// <remarks>
    /// App Settings subclass from Host Settings and so they will
    /// include a superset of configuration sources that also include
    /// all the original bootstrap settings used to resolve the
    /// runtime configuration in case those are needed for reference.
    /// </remarks>
    public class AppSettings : HostSettings
    {
        /// <summary>
        /// Default prefix used to identify environment variables
        /// that can override server runtime app configuration.
        /// </summary>
        public new const string ConfigEnvPrefix = "TUG_CFG_";

        public PullServiceSettings PullService
        { get; set; }
    }
}