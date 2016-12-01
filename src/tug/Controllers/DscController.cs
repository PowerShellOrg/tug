using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using tug.Messages;

namespace tug.Controllers
{
    /// <summary>
    /// A controller that implements the core v2 requests for a
    /// DSC Pull Server, including registration, status checking,
    /// configuration retrieval and module retrieval. 
    /// </summary>
    public class DscController : Controller
    {
        private ILogger<DscController> _logger;
        private IDscHandlerProvider _dscHandlerProvider;
        public DscController(ILogger<DscController> logger,
                IDscHandlerProvider handlerProvider)
        {
            _logger = logger;
            _dscHandlerProvider = handlerProvider;
        }

        [HttpPut]
        [Route("Nodes(AgentId='{AgentId}')")]
        //TODO:  [Authorize]
        public IActionResult RegisterDscAgent(RegisterDscAgentRequest input)
        {
            _logger.LogInformation("\n\n\nPUT: Node registration");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    h.RegisterDscAgent(input.AgentId, input.Body);
                }

                return this.Model(RegisterDscAgentResponse.INSTANCE);
            }

            return base.BadRequest(ModelState);
        }

        [HttpPost]
        [Route("Nodes(AgentId='{AgentId}')/GetDscAction")]
        public IActionResult GetDscAction(GetDscActionRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: DSC action request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                Tuple<DscActionStatus, GetDscActionResponseBody.DetailsItem[]> actionInfo;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    actionInfo = h.GetDscAction(input.AgentId, input.Body);
                }

                var response = new GetDscActionResponse
                {
                    Body = new GetDscActionResponseBody
                    {
                        NodeStatus = actionInfo.Item1,
                        Details = actionInfo.Item2, 
                        // new[]
                        // {
                        //     new GetDscActionResponseBody.DetailsItem
                        //     {
                        //         ConfigurationName = input.AgentId.ToString(),
                        //         Status = DscActionStatus.OK,
                        //     }
                        // },
                    }
                };

                return this.Model(response);
            }

            return base.BadRequest(ModelState);
        }


        [HttpGet]
        [Route("Nodes(AgentId='{AgentId'})/Configurations(ConfigurationName='{ConfigurationName}')/ConfigurationContent")]
        public IActionResult GetConfiguration(GetConfigurationRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: MOF request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}] Configuration=[{input.ConfigurationName}]");
                
                Tuple<string, string, Stream> configInfo;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    configInfo = h.GetConfiguration(input.AgentId, input.ConfigurationName);
                }
                if (configInfo == null)
                    return NotFound();

                var response = new GetConfigurationResponse
                {
                    ChecksumAlgorithmHeader = configInfo.Item1,
                    ChecksumHeader = configInfo.Item2,
                    Configuration = configInfo.Item3,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("Modules(ModuleName='{ModuleName}',ModuleVersion='{ModuleVersion}')/ModuleContent")]
        public IActionResult GetModule(GetModuleRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: Module request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"Module name=[{input.ModuleName}] Version=[{input.ModuleVersion}]");

                Tuple<string, string, Stream> moduleInfo;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    moduleInfo = h.GetModule(input.ModuleName, input.ModuleVersion);
                }
                if (moduleInfo == null)
                    return NotFound();

                var response = new GetModuleResponse
                {
                    ChecksumAlgorithmHeader = moduleInfo.Item1,
                    ChecksumHeader = moduleInfo.Item2,
                    Module = moduleInfo.Item3,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }
    }
}