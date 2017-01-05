using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    // With a little help from:
    //    https://msdn.microsoft.com/en-us/library/ee706570(v=vs.85).aspx

    public class Ps5CustomHost : PSHost
    {
        private readonly Guid _InstanceId = Guid.NewGuid();

        private ILogger _logger;

        private string _Name;
        private PSHostUserInterface _hostUI;

        public Ps5CustomHost(ILogger logger, string name = null,
                PSHostUserInterface hostUI = null)
        {
            _logger = logger;

            if (name == null)
                name = $"{nameof(Ps5CustomHost)}-{Guid.NewGuid()}";
            _Name = name;

            if (hostUI == null)
                hostUI = new Ps5CustomHostUI(_logger);
            _hostUI = hostUI;
        }

        public override CultureInfo CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture;
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                return CultureInfo.CurrentCulture;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                return _InstanceId;
            }
        }

        public override string Name
        {
            get
            {
                return _Name;
            }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return _hostUI;
            }
        }

        public override Version Version
        {
            get
            {
                return typeof(Ps5CustomHost).GetTypeInfo().Assembly.GetName().Version;
            }
        }

        public override void EnterNestedPrompt()
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(EnterNestedPrompt));
            throw new NotImplementedException();
        }

        public override void ExitNestedPrompt()
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(ExitNestedPrompt));
            throw new NotImplementedException();
        }

        public override void NotifyBeginApplication()
        {
            _logger.LogInformation("NOTIFIED:  BeginApplication");
        }

        public override void NotifyEndApplication()
        {
            _logger.LogInformation("NOTIFIED:  EndApplication");            
        }

        public override void SetShouldExit(int exitCode)
        {
            _logger.LogWarning("SHOULD-EXIT:  " + exitCode);
        }
    }
}