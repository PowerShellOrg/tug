using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tug.Messages;
using Tug.Server.Configuration;
using Tug.Util;

namespace Tug.Server.Filters
{
    /// <summary>
    /// An <see cref="IActionFilter">Action Filter</see> that implements
    /// authorization logic according to the DSC Pull Server "Registration
    /// Key Authorization" mechanism.
    /// </summary>
    /// <remarks>
    /// <p><b>NOTE:  This is the original implementaiton of the RegKey Authz
    /// filter which performs its validation in an alternate method.  it
    /// is preserved as an <i>alternative-implementation</i> filter.</b></p>
    ///
    /// Despite the name, this filter is implemented as an MVC Action Filter,
    /// not an Authorization Filter due to the need for access to input data
    /// elements that are processed and available just before invoking the
    /// resolved Controller Action.
    /// </remarks>
    public class DscRegKeyAuthzFilterAlt : IActionFilter
    {
        public const string REG_KEY_PATH = "RegistrationKeyPath";
        public const string REG_SAVE_PATH = "RegistrationSavePath";

        public const string REG_KEY_DEFAULT_FILENAME = "RegistrationKeys.txt";
        public const char REG_KEY_FILE_COMMENT_START = '#';

        public const string SHARED_AUTHORIZATION_PREFIX = "Shared ";

        private ILogger _logger;
        private AuthzSettings _settings;

        private string _regKeyFilePath;
        private string _regSavePath;

        public DscRegKeyAuthzFilterAlt(ILogger<DscRegKeyAuthzFilter> logger,
                IOptions<AuthzSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;

            if (!(_settings.Params?.ContainsKey(REG_KEY_PATH)).GetValueOrDefault())
                throw new InvalidOperationException(
                        /*SR*/"missing required Registration Key Path setting");
            if (!(_settings.Params?.ContainsKey(REG_SAVE_PATH)).GetValueOrDefault())
                throw new InvalidOperationException(
                        /*SR*/"missing required Registration Save Path setting");

            _regKeyFilePath = Path.GetFullPath(_settings.Params[REG_KEY_PATH].ToString());
            _regSavePath = Path.GetFullPath(_settings.Params[REG_SAVE_PATH].ToString());

            if (!File.Exists(_regKeyFilePath) && Directory.Exists(_regKeyFilePath))
            {
                _regKeyFilePath = Path.Combine(_regKeyFilePath, REG_KEY_DEFAULT_FILENAME);
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("resolved reg key file path as [{regKeyFilePath}]", _regKeyFilePath);
                _logger.LogDebug("resolved reg save path as [{regSavePath}]", _regSavePath);
            }

            if (!File.Exists(_regKeyFilePath))
                throw new InvalidOperationException(
                        /*SR*/"could not find registration key file")
                        .WithData(nameof(_regKeyFilePath), _regKeyFilePath);

            if (!Directory.Exists(_regSavePath))
            {
                _logger.LogInformation("registartion save path not found, trying to create");
                var dirInfo = Directory.CreateDirectory(_regSavePath);
                if (!dirInfo.Exists)
                    throw new InvalidOperationException(
                            /*SR*/"could not create registration save directory")
                            .WithData(nameof(_regSavePath), _regSavePath);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var inputArg = context.ActionArguments?.FirstOrDefault();

            // Skip this filter if there is not at least one input argument
            if (inputArg == null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("action does not resolve any arguments; SKIPPING");
                return;
            }

            var input = inputArg.Value.Value as DscRequest;
            if (input == null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("action does not resolve any DscRequest arguments; SKIPPING");
                return;
            }

            var isValid = false;
            var agentId = input.GetAgentId();
            
            // We have 2 scenarios to test for...

            // Scenario #1 - a RegisterDscAgent message in which
            // case we have to validate the request against a valid
            // RegKey and "remember" the Agent ID for future calls
            if (input is RegisterDscAgentRequest)
            {
                var requ = (RegisterDscAgentRequest)input;
                var authz = requ.AuthorizationHeader;
                var xmsdate = requ.MsDateHeader;

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("received authorization header [{authzHeader}]", authz);
                    _logger.LogDebug("received x-ms-date header [{msDateHeader}]", xmsdate);
                }

                // NOTE:  we repeat the following process on every lookup instead
                //        of caching it as a fast and dirty way of picking up any
                //        changes to the file.
                // TODO:  in future preload the file into an array and reload after
                //        listening for and detecting any file changes

                // Resolve reg keys from file as non-blank lines after optional comments
                // (starting with a '#') and any surround whitespace have been stripped
                var regKeys = File.ReadAllLines(_regKeyFilePath)
                        .Select(x => x.Split(REG_KEY_FILE_COMMENT_START)[0].Trim())
                        .Where(x => x.Length > 0);
                var bodyJson = JsonConvert.SerializeObject(requ.Body);
                var bodyBytes = Encoding.UTF8.GetBytes(bodyJson);
                isValid = ValidateRegKeySignature(authz, xmsdate, regKeys, bodyBytes);

                if (isValid)
                {
                    // At this point authorization was successful, so let's remember the Agent ID for future calls
                    var savePath = Path.Combine(_regSavePath, $"{requ.AgentId}.json");
                    File.WriteAllText(savePath, JsonConvert.SerializeObject(requ.Body));
                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("saved registration details to [{savePath}]", savePath);
                }
            }
            else
            {
                // In subsequent calls after the initial reg, we just
                // need to validate that the Agent ID has been registered
                var savePath = Path.Combine(_regSavePath, $"{agentId}.json");
                isValid = File.Exists(savePath);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("looking for registration details at [{savePath}]", savePath);
                    _logger.LogDebug("reg key validation for Agent ID [{agentId}] = [{isValid}]", agentId, isValid);
                }
            }

            if (!isValid)
            {
                _logger.LogWarning("failed RegKey authorization for Agent ID [{agentId}]", agentId);
                context.Result = new UnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        { }

        public static bool ValidateRegKeySignature(string authzHeader, string msDateHeader,
                IEnumerable<string> regKeys, byte[] bodyBytes)
        {
            if (string.IsNullOrEmpty(authzHeader) || !authzHeader.StartsWith(
                    SHARED_AUTHORIZATION_PREFIX, StringComparison.CurrentCultureIgnoreCase))
                throw new InvalidDataException(
                        /*SR*/"registration header is invalid")
                        .WithData(nameof(authzHeader), authzHeader);
            
            authzHeader = authzHeader.Replace(SHARED_AUTHORIZATION_PREFIX, "").Trim();

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bodyBytes);
                var hashB64 = Convert.ToBase64String(hash);
                var toBeSigned = $"{hashB64}\n{msDateHeader}";
                var toBeSignedBytes = Encoding.UTF8.GetBytes(toBeSigned);

                foreach (var line in regKeys)
                {
                    var regKey = line.Trim();
                    var regKeyBytes = Encoding.UTF8.GetBytes(regKey);
                    using (var hmac = new HMACSHA256(regKeyBytes))
                    {
                        var sig = Convert.ToBase64String(hmac.ComputeHash(toBeSignedBytes));
                        if (sig == authzHeader)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}