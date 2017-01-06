using System;
using Microsoft.Extensions.Logging;

namespace Tug.Server.Providers
{
    /// <summary>
    /// A wrapper for <see cref="ILogger"/> that exposes all of the
    /// common logging extension methods as first-class instance
    /// methods.
    /// </summary>
    /// <remarks>
    /// Unfortunately, the PowerShell language does not have native or <i>natural</i>
    /// support for extension methods, which is how the bulk of the logging methods
    /// are implemented for the <c>ILogger</c> interface, so using this class in PS
    /// would be somewhat cumbersome (invoking a static method on an instance).
    /// Instead, this wrapper maps all the common extension methods to first-class
    /// instance methods and makes using it in PowerShell simpler and more natural.
    /// </remarks>
    public class PsLogger : ILogger
    {
        private ILogger _inner;

        public PsLogger(ILogger inner)
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            _inner = inner;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _inner.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _inner.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _inner.Log(logLevel, eventId, state, exception, formatter);
        }

        //
        // Summary:
        //     Formats the message and creates a scope.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to create the scope in.
        //
        //   messageFormat:
        //     Format string of the scope message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        //
        // Returns:
        //     A disposable scope object. Can be null.
        public IDisposable BeginScope(string messageFormat, params object[] args)
        {
            return _inner.BeginScope(messageFormat, args);
        }
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(string message, params object[] args)
        {
            _inner.LogCritical(message, args);
        }
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(EventId eventId, string message, params object[] args)
        {
            _inner.LogCritical(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogCritical(eventId, exception, message, args);
        }
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(string message, params object[] args)
        {
            _inner.LogDebug(message, args);
        }
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(EventId eventId, string message, params object[] args)
        {
            _inner.LogDebug(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogDebug(eventId, exception, message, args);
        }
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(string message, params object[] args)
        {
            _inner.LogError(message, args);
        }
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(EventId eventId, string message, params object[] args)
        {
            _inner.LogError(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogError(eventId, exception, message, args);
        }
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(string message, params object[] args)
        {
            _inner.LogInformation(message, args);
        }
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(EventId eventId, string message, params object[] args)
        {
            _inner.LogInformation(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogInformation(eventId, exception, message, args);
        }
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(string message, params object[] args)
        {
            _inner.LogTrace(message, args);
        }
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(EventId eventId, string message, params object[] args)
        {
            _inner.LogTrace(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogTrace(eventId, exception, message, args);
        }
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(string message, params object[] args)
        {
            _inner.LogWarning(message, args);
        }
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(EventId eventId, string message, params object[] args)
        {
            _inner.LogWarning(eventId, message, args);
        }
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message.
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
        {
            _inner.LogWarning(eventId, exception, message, args);
        }
    }
}