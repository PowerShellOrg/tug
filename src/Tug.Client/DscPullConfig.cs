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
    /// <summary>
    /// Defines configuration settings to control the behavior of a DSC Pull Client.
    /// </summary>
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

        /// <summary>
        /// Defines connection settings for a DSC server endpoint.
        /// </summary>
        /// <remarks>
        /// These settings can be used for defining the endpoint connection settings
        /// for a Configuration Respository, a Module Repository or a Reporting server.
        /// </remarks>
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