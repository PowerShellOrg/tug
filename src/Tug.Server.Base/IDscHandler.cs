/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
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
        void SendReport(Guid agentId, SendReportRequestBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766283.aspx
        /// </summary>
        Stream GetReports(Guid agentId);
    }
}