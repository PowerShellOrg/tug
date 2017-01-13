/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tug.Model;
using Tug.Util;

namespace Tug.Server.Providers
{
    public class Ps5DscHandler : IDscHandler
    {
        public static readonly IEnumerable EMPTY_INPUT = new object[0];

        public const string PS_VAR_HANDLER_LOGGER = "handlerLogger";
        public const string PS_VAR_HANDLER_APP_CONFIGURATION = "handlerAppConfiguration";
        public const string PS_VAR_HANDLER_CONTEXT = "handlerContext";

        private string _bootstrapFullpath;
        private PowerShell _posh;

        public bool IsDisposed
        { get; private set; }

        public ILogger<Ps5DscHandler> Logger
        { get; set; }

        public IConfiguration AppConfig
        { get; set; }

        public string BootstrapPath
        { get; set; } = ".";

        public IEnumerable<string> BootstrapScript
        { get; set; }

        public void Init()
        {
            Logger.LogInformation($"Resolving Boostrap Full Path from Path=[{BootstrapPath}]");
            _bootstrapFullpath = Path.Combine(Directory.GetCurrentDirectory(), BootstrapPath);
            Logger.LogInformation($"Resolved Bootstrap Full Path as [{_bootstrapFullpath}]");

            _posh = PowerShell.Create();
            Logger.LogInformation("Constructed PowerShell execution context");
            
            // Need to create our own runspace so we can pass in our own custom PSHost
            // which ties the PowerShell context to our custom handler environment
            _posh.Runspace = RunspaceFactory.CreateRunspace(new Ps5CustomHost(Logger, name: "PS5Host"));
            _posh.Runspace.Open();

            // Register some context-supporting variables in scope
            _posh.AddCommand("Microsoft.PowerShell.Utility\\Set-Variable");
            _posh.AddParameter("Name", PS_VAR_HANDLER_LOGGER);
            _posh.AddParameter("Option", "ReadOnly");
            _posh.AddParameter("Description", "ILogger to be used by handler cmdlet");
            _posh.AddParameter("Value", new PsLogger(Logger));
            _posh.Invoke();
            _posh.Commands.Clear();

            // Register some context-supporting variables in scope
            _posh.AddCommand("Microsoft.PowerShell.Utility\\Set-Variable");
            _posh.AddParameter("Name", PS_VAR_HANDLER_APP_CONFIGURATION);
            _posh.AddParameter("Option", "ReadOnly");
            _posh.AddParameter("Description", "App-wide configuration (read-only)");
            _posh.AddParameter("Value", new ReadOnlyConfiguration(AppConfig));
            _posh.Invoke();
            _posh.Commands.Clear();

            // Set the CWD for the PowerShell cmdlets to run from
            _posh.AddCommand("Microsoft.PowerShell.Management\\Set-Location");
            _posh.AddArgument(BootstrapPath);
            var result = _posh.Invoke();
            _posh.Commands.Clear();
            Logger.LogInformation("Relocated PWD for current execution context >>>>>>>>>>>>>>");
            foreach (var r in result)
                Logger.LogWarning(">> " + r.ToString());
            
            // If a bootstrap script was provided in the app settings
            // let's invoke it and capture the results for diagnostics
            if (BootstrapScript != null && BootstrapScript.Count() > 0)
            {
                Logger.LogInformation("Bootstrap Script found");
                Logger.LogDebug("--8<---------------------------------");
                foreach (var s in BootstrapScript)
                {
                    Logger.LogDebug(s);
                    _posh.AddScript(s);
                }
                Logger.LogDebug("--------------------------------->8--");
                result = _posh.Invoke();

                _posh.Commands.Clear();

                Logger.LogInformation("Bootstrap Script executed");
                Logger.LogDebug("--8<---------------------------------");
                foreach (var r in result)
                    Logger.LogDebug(">> " + r.ToString());
                Logger.LogDebug("--------------------------------->8--");
            }
        }

        public void RegisterDscAgent(Guid agentId, RegisterDscAgentRequestBody detail)
        {
            // Return value is ignored, if no exceptions are thrown up, we assume success
            ThreadSafeInvokeNoResult("Register-TugNode", agentId, detail);
        }

        public ActionStatus GetDscAction(Guid agentId, GetDscActionRequestBody detail)
        {
            var result = ThreadSafeInvokeSingleOrNoResult<ActionStatus>("Get-TugNodeAction",
                    agentId, detail);

            // TODO:  any additional checks or translations?

            return result;
        }
        public FileContent GetConfiguration(Guid agentId, string configName)
        {
            var result = ThreadSafeInvokeSingleOrNoResult<FileContent>("Get-TugNodeConfiguration",
                    agentId, configName);

            // TODO:  any additional checks or translations?

            return result;
        }

        public FileContent GetModule(Guid? agentId, string moduleName, string moduleVersion)
        {
            var result = ThreadSafeInvokeSingleOrNoResult<FileContent>("Get-TugModule",
                    agentId, moduleName, moduleVersion);

            // TODO:  any additional checks or translations?

            return result;
        }

        public void SendReport(Guid agentId, SendReportRequestBody detail)
        {
            ThreadSafeInvokeNoResult("New-TugNodeReport", agentId, detail);
        }

        public Stream GetReports(Guid agentId)
        {
            // TODO:  this interface is not definitive yet
            var result = ThreadSafeInvokeSingleResult<Stream>("Get-TugNodeReports",
                    agentId);

            return result;
        }

        protected void ThreadSafeInvokeNoResult(string cmd, params object[] args)
        {
            var result = ThreadSafeInvoke<object>(cmd, args);

            if (result != null && result.Count > 0)
                throw new InvalidDataException(/*SR*/"Unexpected result found");
        }

        protected T ThreadSafeInvokeSingleResult<T>(string cmd, params object[] args)
        {
            var result = ThreadSafeInvoke<T>(cmd, args);

            if (result == null || result.Count < 1)
                throw new InvalidDataException(/*SR*/"Missing or empty result");
            if (result.Count > 1)
                throw new InvalidDataException(/*SR*/"Multiple results found");
            
            return result[0];
        }

        protected T ThreadSafeInvokeSingleOrNoResult<T>(string cmd, params object[] args)
            where T : class
        {
            var result = ThreadSafeInvoke<T>(cmd, args);

            if (result != null && result.Count > 1)
                throw new InvalidDataException(/*SR*/"Multiple results found");
            
            return result == null ? null : result[0];
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

                if (Logger.IsEnabled(LogLevel.Trace))
                {
                    Logger.LogTrace($"ThreadSafeInvoke({cmd}) >>>>>>>>>>>>");
                    if (result == null || result.Count == 0)
                        Logger.LogTrace("  (NULL/EMPTY result)");
                    else
                        foreach (var r in result)
                            Logger.LogTrace("  >> {resultItem}", r);
                }

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
