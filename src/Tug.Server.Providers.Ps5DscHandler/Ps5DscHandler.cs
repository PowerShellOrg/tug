/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using Microsoft.Extensions.Logging;
using Tug.Model;

namespace Tug.Server.Providers
{
    public class Ps5DscHandler : IDscHandler
    {
        public static readonly IEnumerable EMPTY_INPUT = new object[0];

        private string _bootstrapFullpath;
        private PowerShell _posh;

        public bool IsDisposed
        { get; private set; }

        public ILogger<Ps5DscHandler> Logger
        { get; set; }

        public string BootstrapPath
        { get; set; } = ".";

        public string BootstrapScript
        { get; set; }

        public void Init()
        {
            Logger.LogInformation($"Resolving Boostrap Full Path from Path=[{BootstrapPath}]");
            _bootstrapFullpath = Path.Combine(Directory.GetCurrentDirectory(), BootstrapPath);
            Logger.LogInformation($"Resolved Bootstrap Full Path as [{_bootstrapFullpath}]");

            _posh = PowerShell.Create();
            Logger.LogInformation("Constructed PowerShell execution context");
            
            _posh.AddCommand("Microsoft.PowerShell.Management\\Set-Location");
            _posh.AddArgument(BootstrapPath);
            var result = _posh.Invoke();
            _posh.Commands.Clear();
            Logger.LogInformation("Relocated PWD for current execution context >>>>>>>>>>>>>>");
            foreach (var r in result)
                Logger.LogWarning(">> " + r.ToString());
            

            if (!string.IsNullOrEmpty(BootstrapScript))
            {
                Logger.LogInformation("Bootstrap Script found");
                _posh.AddScript(BootstrapScript);
                result = _posh.Invoke();
                _posh.Commands.Clear();
                Logger.LogInformation("Bootstrap Script executed >>>>>>>>>>>>");
                foreach (var r in result)
                    Logger.LogWarning(">> " + r.ToString());
            }
        }

        public void RegisterDscAgent(Guid agentId, RegisterDscAgentRequestBody detail)
        {
            // Return value is ignored, if no exceptions are thrown up, we assume success
            ThreadSafeInvoke<object>("Register-TugNode", agentId, detail);

            //throw new NotImplementedException();
        }

        public ActionStatus GetDscAction(Guid agentId, GetDscActionRequestBody detail)
        {
            var result = ThreadSafeInvoke<object>("Get-TugNodeAction", agentId, detail);

            //throw new NotImplementedException();

            return new ActionStatus
            {
                NodeStatus = DscActionStatus.OK,
            };
        }
        public FileContent GetConfiguration(Guid agentId, string configName)
        {
            // 
            // ThreadSafeInvoke<object>("Get-TugNodeConfiguration", agentId, configName);
            // 
            throw new NotImplementedException();
        }

        public FileContent GetModule(string moduleName, string moduleVersion)
        {
            // 
            // ThreadSafeInvoke<object>("Get-TugModule", moduleName, moduleVersion);
            // 
            throw new NotImplementedException();
        }

        public void SendReport(Guid agentId, SendReportRequestBody detail)
        {
            // 
            // ThreadSafeInvoke<object>("New-TugNodeReport", agentId, reportContent, reserved);
            // 
            throw new NotImplementedException();
        }

        public Stream GetReports(Guid agentId)
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
                
                var result = _posh.Invoke<T>(EMPTY_INPUT);
                Logger.LogInformation($"ThreadSafeInvoke({cmd}) >>>>>>>>>>>>");
                foreach (var r in result)
                    Logger.LogWarning(">> " + r.ToString());

                return result;
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

                    // TODO:  THIS NEEDS TO BE REFACTORED AFTER FIGURING OUT LIFECYLCE OF HANDLERS
                    // if (_posh != null)
                    //     _posh.Dispose();
                    // _posh = null;
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
