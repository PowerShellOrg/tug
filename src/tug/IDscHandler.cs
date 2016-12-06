using System;
using System.Collections.Generic;
using System.IO;
using tug.Messages;

namespace tug
{
    public interface IDscHandlerProvider
    {
        IEnumerable<string> GetParameters();

        IDscHandler GetHandler(IDictionary<string, object> initParams);
    }

    /// <summary>
    /// Defines the operations and primitives needed to be handled
    /// by a DSC implementation.
    /// </summary>
    /// <remarks>
    /// The operations are defined based on the Pull Server
    /// protocol specification as found
    /// <see href="https://msdn.microsoft.com/en-us/library/dn366007.aspx">here</see>.
    /// </remarks>
    public interface IDscHandler : IDisposable
    {
        bool IsDisposed
        { get; }

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt590247.aspx
        /// </summary>
        void RegisterDscAgent(Guid agentId,
                Messages.RegisterDscAgentRequestBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766279.aspx
        /// </summary>
        Tuple<DscActionStatus, GetDscActionResponseBody.DetailsItem[]> GetDscAction(Guid agentId,
                Messages.GetDscActionRequestBody detail);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766328.aspx
        /// </summary>
        Tuple<string, string, Stream> GetConfiguration(Guid agentId, string configName);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766336.aspx
        /// </summary>
        Tuple<string, string, Stream> GetModule(string moduleName, string moduleVersion);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766272.aspx
        /// </summary>
        void SendReport(Guid agentId, Stream reportContent,
                Messages.SendReportRequest reserved);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766283.aspx
        /// </summary>
        Stream GetReports(Guid agentId,
                Messages.GetReportsRequest reserved);
    }
}