/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

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

        public string FriendlyName
        { get; set; }

        public string Issuer
        { get; set; }

        public string NotAfter
        { get; set; }

        public string NotBefore
        { get; set; }

        public string Subject
        { get; set; }

        public string PublicKey
        { get; set; }

        public string Thumbprint
        { get; set; }

        // This *MUST* be an int or RegisterDscAction will fail with a
        // 401 Unauthorized error and eroneously report an invalid
        // Registration Key -- as HOURS of debugging has proven!
        public int Version
        { get; set; }
    }
}
