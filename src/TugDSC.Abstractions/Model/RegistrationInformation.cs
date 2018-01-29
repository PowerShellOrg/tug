/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.ComponentModel.DataAnnotations;

namespace TugDSC.Model
{
    public class RegistrationInformation : Util.ExtDataIndexerBase
    {
        // NOTE:  DO NOT CHANGE THE ORDER OF THESE PROPERTIES!!!
        // Apparently the order of these properties is important
        // to successfully fulfill the RegKey authz requirements

        [Required]
        public CertificateInformation CertificateInformation
        { get; set; }

        [Required]
        public string RegistrationMessageType
        { get; set; }
    }
}