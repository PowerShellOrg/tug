// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Tug.Messages;
using Tug.Model;
using Tug.Server.Mvc;
using Tug.Server.Util;

namespace Tug.Server.Controllers
{
    /// <summary>
    /// A controller that implements the core v2 requests for a
    /// DSC Pull Server, including registration, status checking,
    /// configuration retrieval and module retrieval. 
    /// </summary>
    public class DscController : Controller
    {
        private ILogger<DscController> _logger;
        private DscHandlerHelper _dscHelper;
        private IDscHandler _dscHandler;

        public DscController(ILogger<DscController> logger,
                DscHandlerHelper dscHelper)
        {
            _logger = logger;
            _dscHelper = dscHelper;
            _dscHandler = _dscHelper.DefaultHandler;
        }

        [HttpPut]
        [Route(RegisterDscAgentRequest.ROUTE,
            Name = RegisterDscAgentRequest.ROUTE_NAME)]
        [ActionName(nameof(RegisterDscAgent))]
        public IActionResult RegisterDscAgent(RegisterDscAgentRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(RegisterDscAgent)}:  {RegisterDscAgentRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");
                _dscHandler.RegisterDscAgent(input.AgentId.Value, input.Body);

                return this.Model(RegisterDscAgentResponse.INSTANCE);
            }

            return base.BadRequest(ModelState);
        }

        [HttpPost]
        [Route(GetDscActionRequest.ROUTE,
            Name = GetDscActionRequest.ROUTE_NAME)]
        [ActionName(nameof(GetDscAction))]
        public IActionResult GetDscAction(GetDscActionRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetDscAction)}:  {GetDscActionRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                var actionInfo = _dscHandler.GetDscAction(input.AgentId.Value, input.Body);
                if (actionInfo == null)
                    return NotFound();
                
                var response = new GetDscActionResponse
                {
                    Body = new GetDscActionResponseBody
                    {
                        NodeStatus = actionInfo.NodeStatus,
                        Details = actionInfo.ConfigurationStatuses?.ToArray(),
                    }
                };

                return this.Model(response);
            }

            return base.BadRequest(ModelState);
        }


        [HttpGet]
        [Route(GetConfigurationRequest.ROUTE,
            Name = GetConfigurationRequest.ROUTE_NAME)]
        [ActionName(nameof(GetConfiguration))]
        public IActionResult GetConfiguration(GetConfigurationRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetConfiguration)}:  {GetConfigurationRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}] Configuration=[{input.ConfigurationName}]");
                
                var configContent = _dscHandler.GetConfiguration(input.AgentId.Value,
                        // TODO:
                        // Strictly speaking, this may not be how the DSCPM
                        // protocol is supposed to resolve the config name
                        input.ConfigurationName ?? input.ConfigurationNameHeader);
                if (configContent == null)
                    return NotFound();

                var response = new GetConfigurationResponse
                {
                    ChecksumAlgorithmHeader = configContent.ChecksumAlgorithm,
                    ChecksumHeader = configContent.Checksum,
                    Configuration = configContent.Content,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route(GetModuleRequest.ROUTE,
            Name = GetModuleRequest.ROUTE_NAME)]
        [ActionName(nameof(GetModule))]
        public IActionResult GetModule(GetModuleRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetModule)}:  {GetModuleRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"Module name=[{input.ModuleName}] Version=[{input.ModuleVersion}]");

                var moduleContent = _dscHandler.GetModule(input.GetAgentId(),
                        input.ModuleName, input.ModuleVersion);
                if (moduleContent == null)
                    return NotFound();

                var response = new GetModuleResponse
                {
                    ChecksumAlgorithmHeader = moduleContent.ChecksumAlgorithm,
                    ChecksumHeader = moduleContent.Checksum,
                    Module = moduleContent.Content,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }
    }
}