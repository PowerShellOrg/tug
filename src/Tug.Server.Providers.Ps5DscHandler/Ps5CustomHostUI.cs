using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    // With a little help from:
    //    https://msdn.microsoft.com/en-us/library/ee706570(v=vs.85).aspx

    public class Ps5CustomHostUI : PSHostUserInterface
    {
        private ILogger _logger;
        private PSHostRawUserInterface _RawUI;

        public Ps5CustomHostUI(ILogger logger,
                PSHostRawUserInterface rawUI = null)
        {
            _logger = logger;

            if (rawUI == null)
                rawUI = new Ps5CustomHostRawUI(_logger);
            
            _RawUI = rawUI;
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                return _RawUI;
            }
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(Prompt));
            throw new NotImplementedException();
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(PromptForChoice));
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(PromptForCredential));
            throw new NotImplementedException();
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(PromptForCredential));
            throw new NotImplementedException();
        }

        public override string ReadLine()
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(ReadLine));
            throw new NotImplementedException();
        }

        public override SecureString ReadLineAsSecureString()
        {
            _logger.LogError("NOT IMPLEMENTED: " + nameof(ReadLineAsSecureString));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes characters to the output display of the host.
        /// </summary>
        /// <param name="value">The characters to be written.</param>
        public override void Write(string value)
        {
            Console.Write(value);
        }

        /// <summary>
        /// Writes characters to the output display of the host and specifies the 
        /// foreground and background colors of the characters. This implementation 
        /// ignores the colors.
        /// </summary>
        /// <param name="foregroundColor">The color of the characters.</param>
        /// <param name="backgroundColor">The backgound color to use.</param>
        /// <param name="value">The characters to be written.</param>
        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            Console.Write(value);
        }

        public override void WriteDebugLine(string message)
        {
            _logger.LogDebug(message);
        }

        public override void WriteErrorLine(string value)
        {
            _logger.LogError(value);
        }

        public override void WriteLine()
        {
            _logger.LogInformation("");
        }

        public override void WriteLine(string value)
        {
            _logger.LogInformation(value);
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            _logger.LogTrace("PROGRESS: {sourceId} {record}", sourceId, record);
        }

        public override void WriteVerboseLine(string message)
        {
            _logger.LogTrace(message);
        }

        public override void WriteWarningLine(string message)
        {
            _logger.LogError(message);
        }
    }
}