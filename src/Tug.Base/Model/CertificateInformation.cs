// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System.ComponentModel.DataAnnotations;

namespace Tug.Model
{
    public class CertificateInformation : Util.ExtDataIndexerBase
    {
        public CertificateInformation()
        { }

        public CertificateInformation(CertificateInformation copyFrom)
        {
            this.FriendlyName = copyFrom.FriendlyName;
            this.Issuer = copyFrom.Issuer;
            this.NotAfter = copyFrom.NotAfter;
            this.NotBefore = copyFrom.NotBefore;
            this.Subject = copyFrom.Subject;
            this.PublicKey = copyFrom.PublicKey;
            this.Thumbprint = copyFrom.Thumbprint;
        }

        // NOTE:  DO NOT CHANGE THE ORDER OF THESE PROPERTIES!!!
        // Apparently the order of these properties is important
        // to successfully fulfill the RegKey authz requirements

        [Required]
        public string FriendlyName
        { get; set; }

        [Required]
        public string Issuer
        { get; set; }

        [Required]
        public string NotAfter
        { get; set; }

        [Required]
        public string NotBefore
        { get; set; }

        [Required]
        public string Subject
        { get; set; }

        [Required]
        public string PublicKey
        { get; set; }

        [Required]
        public string Thumbprint
        { get; set; }

        // This *MUST* be an int or RegisterDscAction will fail with a
        // 401 Unauthorized error and eroneously report an invalid
        // Registration Key -- as HOURS of debugging has proven!
        [Required]
        public int Version
        { get; set; }
    }
}
