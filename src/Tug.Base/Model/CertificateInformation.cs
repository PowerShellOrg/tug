/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

namespace Tug.Model
{
    public class CertificateInformation
    {
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

        public string Version
        { get; set; }
    }
}
