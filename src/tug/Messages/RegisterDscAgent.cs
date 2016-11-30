using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace tug.Messages
{
    public class RegisterDscAgentRequest : DscAgentRequest
    {
        [FromBody]
        [Required]
        public RegisterDscAgentRequestBody Body
        { get; set; }
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/dn365245.aspx 
    /// </summary>
    public class RegisterDscAgentRequestBody
    {
        public AgentInformationBody AgentInformation
        { get; set; }

        public string[] ConfigurationNames
        { get; set; }

        public RegistrationInformationBody RegistrationInformation
        { get; set; }

        public class AgentInformationBody
        {
            public string LCMVersion
            { get; set; }

            public string NodeName
            { get; set; }

            public string IPAddress
            { get; set; }
        }

        public class RegistrationInformationBody
        {
            public CertificateInformationBody CertificateInformation
            { get; set; }

            public string RegistrationMessageType
            { get; set; }
        }

        public class CertificateInformationBody
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
}