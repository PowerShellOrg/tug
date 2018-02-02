/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licensed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections.Generic;
using System.Net;
using TugDSC.Model;

namespace TugDSC.Client.CLIApp.Configuration
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
        /// Defines configuration settings that control sending reports
        /// for status, configuration details and errors.
        /// </summary>
        /// <returns></returns>
        public SendReportConfig SendReport
        { get; set; }

        /// <summary>
        /// Defines connection settings for a DSC server endpoint.
        /// </summary>
        /// <remarks>
        /// These settings can be used for defining the endpoint connection settings
        /// for a Configuration Repository, a Module Repository or a Reporting server.
        /// </remarks>
        public class ServerConfig
        {
            public Uri ServerUrl
            { get; set; }

            public string RegistrationKey
            { get; set; }

            /// Configuration *must* qualify this interface instance resolution by using a
            /// <c>$type</c> child property which will specify the concrete type to create.
            public IWebProxy Proxy
            { get; set; }
        }

        /// <summary>
        /// Defines the settings that influence behavior for sending
        /// reports.
        /// </summary>
        public class SendReportConfig
        {
            /// <summary>
            /// Assign this token to any datetime field to automatically
            /// populate it with a computed current timestamp in the
            /// standard report format for dates.
            /// </summary>
            public const string DATETIME_NOW_TOKEN = "%NOW%";

            /// <summary>
            /// Defines the elements of a <c>SendReport</c> request that
            /// will be automatically populated for all sent messages unless
            /// overridden.
            /// </summary>
            public SendReportBody CommonDefaults
            { get; set; }

            /// <summary>
            /// Represents named profiles that define a combination of
            /// elements for a particular report type.  Each profile is
            /// merged over the <see cref="CommonDefaults"/> to form a
            /// resultant base set of elements for the <c>SendReport</c>
            /// request.
            /// </summary>
            public Dictionary<string, SendReportBody> Profiles
            { get; set; }
        }
    }
}