/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Model
{
    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/dn365245.aspx 
    /// </summary>
    public class RegisterDscAgentRequestBody : Util.ExtDataIndexerBase
    {
        // NOTE:  DO NOT CHANGE THE ORDER OF THESE PROPERTIES!!!
        // Apparently the order of these properties is important
        // to successfully fulfill the RegKey authz requirements

        public AgentInformation AgentInformation
        { get; set; }

        public string[] ConfigurationNames
        { get; set; }

        public RegistrationInformation RegistrationInformation
        { get; set; }
    }
}