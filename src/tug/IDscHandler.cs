using System;
using System.IO;

namespace tug
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
    public interface IDscHandler : IDisposable
    {
        bool IsDisposed
        { get; }

        void Init();

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt590247.aspx
        /// </summary>
        void RegisterDscAgent(Guid agentId,
                Messages.RegisterDscAgentRequest reserved);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766328.aspx
        /// </summary>
        Stream GetConfiguration(Guid agentId, string configName,
                Messages.GetConfigurationRequest reserved);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766336.aspx
        /// </summary>
        Stream GetModule(string moduleName, string moduleVersion,
                Messages.GetModuleRequest reserved);

        /// <summary>
        /// https://msdn.microsoft.com/en-us/library/mt766279.aspx
        /// </summary>
        void GetDscAction(Guid agentId,
                Messages.GetDscActionRequest reserved);

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