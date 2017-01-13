using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Headers;
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
    /// Despite the name, this filter is implemented as an MVC Action Filter,
    /// not an Authorization Filter due to the need for access to input data
    /// elements that are processed and available just before invoking the
    /// resolved Controller Action.
    /// </remarks>
    public class DscRegKeyAuthzFilter : IAuthorizationFilter, IActionFilter
    {
        public const string REG_KEY_PATH = "RegistrationKeyPath";
        public const string REG_SAVE_PATH = "RegistrationSavePath";

        public const string REG_KEY_DEFAULT_FILENAME = "RegistrationKeys.txt";
        public const char REG_KEY_FILE_COMMENT_START = '#';

        public const string SHARED_AUTHORIZATION_PREFIX = "Shared ";

        private const string HTTP_CONTEXT_ITEM_AGENT_REG_KEY = nameof(DscRegKeyAuthzFilter)
                + ":AgentRegKey";

        private ILogger _logger;
        private AuthzSettings _settings;

        private string _regKeyFilePath;
        private string _regSavePath;

        public DscRegKeyAuthzFilter(ILogger<DscRegKeyAuthzFilter> logger,
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

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var routeName = context?.ActionDescriptor?.AttributeRouteInfo?.Name;
            var body = context?.HttpContext?.Request?.Body;

            if (RegisterDscAgentRequest.ROUTE_NAME == routeName)
            {
                // The Register DSC message is where we try to validate
                // the request's body against a valid Registration Key
                // so we're going to pre-compute the hash of the body
                // early in the request pipeline so that we can make
                // use of it later on to compute the full HMAC sigs

                if (body == null)
                {
                    _logger.LogError("agent registration request did not provide a request body");
                    context.Result = new BadRequestResult();
                    return;
                }

                var authzHeader = (string)context.HttpContext.Request?.Headers[
                        nameof(HttpRequestHeaders.Authorization)];
                var msDateHeader = (string)context.HttpContext.Request?.Headers[
                        DscRequest.X_MS_DATE_HEADER];

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("received authorization header [{authzHeader}]", authzHeader);
                    _logger.LogDebug("received x-ms-date header [{msDateHeader}]", msDateHeader);
                }
                
                if (string.IsNullOrEmpty(authzHeader))
                {
                    _logger.LogError("agent registration request did not provide an authorization header");
                    context.Result = new UnauthorizedResult();
                    return;
                }

                if (string.IsNullOrEmpty(msDateHeader))
                {
                    _logger.LogError("agent registration request did not provide an MS Date");
                    context.Result = new BadRequestResult();
                    return;
                }

                // Make sure the MS Date is in the proper format
                var msDateValue = DateTime.ParseExact(msDateHeader, DscRequest.X_MS_DATE_FORMAT,
                        CultureInfo.CurrentCulture);
                // Make sure the MS Date is reasonbly recent -- TODO:  app setting?
                var msDateEpoch = DateTime.UtcNow;
                var msDateDiff = msDateEpoch.Subtract(msDateValue);
                var minTimeSpan = TimeSpan.FromSeconds(-30);
                var maxTimeSpan = TimeSpan.FromSeconds(30);
                if (msDateDiff < minTimeSpan || msDateDiff > maxTimeSpan)
                {
                    _logger.LogError("agent registration request provided an out-of-range MS Date",
                            DscRequest.X_MS_DATE_HEADER);
                    context.Result = new BadRequestResult();
                    return;
                }

                // Resolve reg keys from file as non-blank lines after optional comments
                // (starting with a '#') and any surround whitespace have been stripped
                var regKeys = File.ReadAllLines(_regKeyFilePath)
                        .Select(x => x.Split(REG_KEY_FILE_COMMENT_START)[0].Trim())
                        .Where(x => x.Length > 0);

                using (var ms = new MemoryStream())
                {
                    body.CopyTo(ms);

                    var bodyBytes = ms.ToArray();
                    var agentRegKey = ValidateRegKeySignature(authzHeader, msDateHeader,
                            regKeys, bodyBytes);

                    if (string.IsNullOrEmpty(agentRegKey))
                    {
                        _logger.LogError("agent registration request failed to match any registration key");
                        context.Result = new UnauthorizedResult();
                        return;
                    }

                    // Remember the reg key for later in the request pipeline (see down below)
                    context.HttpContext.Items[HTTP_CONTEXT_ITEM_AGENT_REG_KEY] = agentRegKey;

                    // Finally, since we *ate* up the body in order to compute the
                    // signature we need to replace it with another copy, so that
                    // it can be model-bound before invoking the action method
                    context.HttpContext.Request.Body = new MemoryStream(bodyBytes);
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var routeName = context?.ActionDescriptor?.AttributeRouteInfo?.Name;
            var input = (context.ActionArguments?.FirstOrDefault())?.Value;
            var dscRequ = input as DscRequest;

            if (dscRequ == null)
            {
                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogTrace("action does not resolve any DscRequest arguments; SKIPPING");
                return;
            }

            var agentId = dscRequ.GetAgentId();
            if (agentId == null || agentId == Guid.Empty)
            {
                _logger.LogError("DSC request Agent ID is missing or invalid");
                context.Result = new BadRequestResult();
                return;
            }

            if (RegisterDscAgentRequest.ROUTE_NAME == routeName)
            {
                // The Register DSC message is where we try to validate
                // the request's body against a valid Registration Key
                var regRequ = (RegisterDscAgentRequest)dscRequ;
                var agentRegKey = context.HttpContext.Items[HTTP_CONTEXT_ITEM_AGENT_REG_KEY]
                        as string;

                if (regRequ == null)
                {
                    _logger.LogError("agent registration request is missing expected request input");
                    context.Result = new BadRequestResult();
                    return;
                }

                if (string.IsNullOrEmpty(agentRegKey))
                {
                    _logger.LogError("agent registration request did not match any valid registration key");
                    context.Result = new UnauthorizedResult();
                    return;
                }

                // At this point authorization was successful, so let's remember the Agent ID for future calls
                var regKeySavePath = Path.Combine(_regSavePath, $"{agentId}.regkey");
                var detailSavePath = Path.Combine(_regSavePath, $"{agentId}.json");
                File.WriteAllText(regKeySavePath, agentRegKey);
                File.WriteAllText(detailSavePath, JsonConvert.SerializeObject(regRequ.Body));
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("saved registration key to [{savePath}]", regKeySavePath);
                    _logger.LogDebug("saved registration details to [{savePath}]", detailSavePath);
                }
            }
            else
            {
                // For all other requests, if they are valid DSC
                // messages we just need to validate that they are
                // associated with a previously registered Agent ID
                // so validate that the Agent ID has been registered
                var savePath = Path.Combine(_regSavePath, $"{agentId}.json");
                var isValid = File.Exists(savePath);

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("looking for registration details at [{savePath}]", savePath);
                    _logger.LogDebug("reg key validation for Agent ID [{agentId}] = [{isValid}]", agentId, isValid);
                }

                if (!isValid)
                {
                    _logger.LogWarning("failed RegKey authorization for Agent ID [{agentId}]", agentId);
                    context.Result = new UnauthorizedResult();
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        { }

        public static string ValidateRegKeySignature(string authzHeader, string msDateHeader,
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

                foreach (var rk in regKeys)
                {
                    var regKey = rk.Trim();
                    var regKeyBytes = Encoding.UTF8.GetBytes(regKey);
                    using (var hmac = new HMACSHA256(regKeyBytes))
                    {
                        var sig = Convert.ToBase64String(hmac.ComputeHash(toBeSignedBytes));
                        if (sig == authzHeader)
                        {
                            return rk;
                        }
                    }
                }
            }
            
            return null;
        }
    }
}