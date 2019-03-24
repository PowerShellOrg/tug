// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using Tug.Ext;
using Tug.Model;

namespace Tug.Server
{
    /// <summary>
    /// Defines the operations and primitives needed to be handled
    /// by a DSC implementation.
    /// </summary>
    /// <remarks>
    /// The operations are defined based on the Pull Server
    /// protocol specification as found
    /// <see href="https://msdn.microsoft.com/en-us/library/dn366007.aspx">here</see>.
    /// </remarks>
    public interface IDscHandler : IProviderProduct
    {
        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt590247.aspx
        /// </summary>
        void RegisterDscAgent(Guid agentId,
                RegisterDscAgentRequestBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766279.aspx
        /// </summary>
        ActionStatus GetDscAction(Guid agentId,
                GetDscActionRequestBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766328.aspx
        /// </summary>
        FileContent GetConfiguration(Guid agentId, string configName);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766336.aspx
        /// </summary>
        FileContent GetModule(Guid? agentId, string moduleName, string moduleVersion);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766272.aspx
        /// </summary>
        void SendReport(Guid agentId, SendReportBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766283.aspx
        /// </summary>
        IEnumerable<SendReportBody> GetReports(Guid agentId, Guid? jobId);
    }
}