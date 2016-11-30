using System;
using System.IO;
using Newtonsoft.Json;
using tug.Messages;

namespace tug.Providers
{
    public class BasicDscHandler : IDscHandler
    {
        public const string DEFAULT_WORK_FOLDER = ".\\DscRepo";

        private string _workPath;
        private string _nodeRegsPath;
        private string _nodeConfigsPath;
        private string _modulesPath;

        public string WorkFolder
        { get; set; } = DEFAULT_WORK_FOLDER; 

        public bool IsDisposed
        { get; private set; }

        public void Init()
        {
            _workPath = Path.Combine(Directory.GetCurrentDirectory(), WorkFolder);
            _nodeRegsPath = Path.Combine(_workPath, "node_regs");
            _nodeConfigsPath = Path.Combine(_workPath, "node_configs");
            _modulesPath = Path.Combine(_workPath, "modules");

            Directory.CreateDirectory(_nodeRegsPath);
            Directory.CreateDirectory(_nodeConfigsPath);
            Directory.CreateDirectory(_modulesPath);
        }

        public void RegisterDscAgent(Guid agentId,
                RegisterDscAgentRequest reserved)
        {
            var regPath = Path.Combine(_nodeRegsPath, $"{agentId}.json");
            if (File.Exists(regPath))
                throw new Exception("agent ID already registered");
            
            File.WriteAllText(regPath, JsonConvert.SerializeObject(reserved));
        }

        public Stream GetConfiguration(Guid agentId, string configName,
                GetConfigurationRequest reserved)
        {
            var configPath = Path.Combine(_nodeConfigsPath, $"{agentId}.json");
            if (!File.Exists(configPath))
                return null;

            return File.OpenRead(configPath);
        }

        public Stream GetModule(string moduleName, string moduleVersion,
                GetModuleRequest reserved)
        {
            var modulePath = Path.Combine(_modulesPath, $"{moduleName}/{moduleVersion}");
            if (!File.Exists(modulePath))
                return null;
            
            return File.OpenRead(modulePath);
        }

        public void GetDscAction(Guid agentId,
            GetDscActionRequest reserved)
        {
            throw new NotImplementedException();
        }

        public void SendReport(Guid agentId, Stream reportContent,
                SendReportRequest reserved)
        {
            throw new NotImplementedException();
        }

        public Stream GetReports(Guid agentId,
                GetReportsRequest reserved)
        {
            throw new NotImplementedException();
        }


        #region -- IDisposable Support --

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BasicDscHandler() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion -- IDisposable Support --
    }
}