using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    public class Ps5DscHandler : IDisposable // IDscHandlerProvider
    {
        public static readonly IEnumerable EMPTY_INPUT = new object[0];

        private string _bootstrapFullpath;
        private PowerShell _posh;

        public bool IsDisposed
        { get; private set; }

        public ILogger<Ps5DscHandler> LOG
        { get; set; }

        public string BootstrapPath
        { get; set; }

        public string BootstrapScript
        { get; set; }

        public void Init()
        {
            LOG.LogInformation($"Resolving Boostrap Full Path from Path=[{BootstrapPath}]");
            _bootstrapFullpath = Path.Combine(Directory.GetCurrentDirectory(), BootstrapPath);
            LOG.LogInformation($"Resolved Bootstrap Full Path as [{_bootstrapFullpath}]");

            _posh = PowerShell.Create();
            LOG.LogInformation("Constructed PowerShell execution context");
            
            _posh.AddCommand("Microsoft.PowerShell.Management\\Set-Location");
            _posh.AddArgument(BootstrapPath);
            _posh.Invoke();
            _posh.Commands.Clear();
            LOG.LogInformation("Relocated PWD for current execution context");
        }

        public void RegisterDscAgent(Guid agentId,
                /*RegisterDscAgentRequestBody*/ object detail)
        {
            // Return value is ignored, if no exceptions are thrown up, we assume success
            ThreadSafeInvoke<object>("Register-TugNode", agentId, detail);
        }

        public /*Tuple<DscActionStatus, GetDscActionResponseBody.DetailsItem[]>*/
        Tuple<object, object> GetDscAction(Guid agentId,
                /*GetDscActionRequestBody*/ object detail)
        {
            var result = ThreadSafeInvoke<object>("Get-TugNodeAction", agentId, detail);
            var resultArr = result.ToArray();
            return Tuple.Create(resultArr[0], resultArr[1]);
        }
        public Tuple<string, string, Stream> GetConfiguration(Guid agentId, string configName)
        {
            // 
            // ThreadSafeInvoke<object>("Get-TugNodeConfiguration", agentId, configName);
            // 
            throw new NotImplementedException();
        }

        public Tuple<string, string, Stream> GetModule(string moduleName, string moduleVersion)
        {
            // 
            // ThreadSafeInvoke<object>("Get-TugModule", moduleName, moduleVersion);
            // 
            throw new NotImplementedException();
        }

        public void SendReport(Guid agentId, Stream reportContent,
                /*SendReportRequest*/ object reserved)
        {
            // 
            // ThreadSafeInvoke<object>("New-TugNodeReport", agentId, reportContent, reserved);
            // 
            throw new NotImplementedException();
        }

        public Stream GetReports(Guid agentId,
                /*GetReportsRequest*/ object reserved)
        {
            // 
            // ThreadSafeInvoke<object>("Get-TugNodeReports", agentId, reserved);
            // 
            throw new NotImplementedException();
        }
                

        protected Collection<T> ThreadSafeInvoke<T>(string cmd, params object[] args)
        {
            // TODO:  It doesn't look like a PowerShell instance is thread-safe
            // so we're going to have to design this to either do a lot of locking
            // around PS invocations which won't scale too well or setup a mechanism
            // to support pooling of PS contexts

            lock (_posh)
            {
                _posh.Commands.Clear();
                _posh.AddCommand(cmd);
                foreach (var a in args)
                    _posh.AddArgument(a);
                
                return _posh.Invoke<T>(EMPTY_INPUT);
            }
        }

        #region -- IDisposable Support --
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_posh != null)
                        _posh.Dispose();
                    _posh = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                IsDisposed  = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Ps5DscHandler() {
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
