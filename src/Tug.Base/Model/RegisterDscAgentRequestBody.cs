/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

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

        [Required]
        public AgentInformation AgentInformation
        { get; set; }

        // Based on testing and observation, this property
        // is completely omitted when it has no value
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] ConfigurationNames
        { get; set; }

        [Required]
        public RegistrationInformation RegistrationInformation
        { get; set; }
    }
}