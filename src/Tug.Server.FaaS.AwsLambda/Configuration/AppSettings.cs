/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Server.FaaS.AwsLambda.Configuration
{
    public class AppSettings
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