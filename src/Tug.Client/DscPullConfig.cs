/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.Net;
using Tug.Model;

namespace Tug.Client
{
    public class DscPullConfig
    {
        public Guid AgentId
        { get; set; }

        public AgentInformation AgentInformation
        { get; set; }

        public CertificateInformation CertificateInformation
        { get; set; }

        public IEnumerable<string> ConfigurationNames
        { get; set; }

        public ServerConfig ConfigurationRepositoryServer
        { get; set; }

        public ServerConfig ResourceRepositoryServer
        { get; set; }

        public ServerConfig ReportServer
        { get; set; }

        public class ServerConfig
        {
            public Uri ServerUrl
            { get; set; }

            public string RegistrationKey
            { get; set; }
            
            public IWebProxy Proxy
            { get; set; }
        }
    }
}