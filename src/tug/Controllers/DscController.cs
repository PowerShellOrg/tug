using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private IDscHandler _dscHandler;

        public DscController(IDscHandler handler)
        {
            _dscHandler = handler;
        }

        [HttpPut]
        [Route("Nodes(AgentId='{AgentId}')")]
        //TODO:  [Authorize]
        public IActionResult RegisterDscAgent(RegisterDscAgentRequest input)
        {
            if (ModelState.IsValid)
            {
                _dscHandler.RegisterDscAgent(input.AgentId, input);

                Response.Headers.Add(DscResponse.PROTOCOL_VERSION_HEADER,
                        DscResponse.PROTOCOL_VERSION_VALUE);
                return NoContent();
                // return Json(new
                // {
                //     AgentId = input.AgentId,
                //     Message = "Woohoo!  You're in the 'in' crowd now!",
                // });
            }

            return base.BadRequest(ModelState);
        }

        [HttpPost]
        public IActionResult GetDscAction(GetDscActionRequest input)
        {
            if (ModelState.IsValid)
            {
                return Json(new GetDscActionResponseBody
                {
                    NodeStatus = DscActionStatus.OK,
                });
            }

            return base.BadRequest(ModelState);
        }


        [HttpGet]
        [Route("Nodes(AgentId='{AgentId'})/Configurations(ConfigurationName='{ConfigurationName}')/ConfigurationContent")]
        public IActionResult GetConfiguration(GetConfigurationRequest input)
        {
            if (ModelState.IsValid)
            {
                var s = _dscHandler.GetConfiguration(input.AgentId, input.ConfigurationName, input);
                if (s == null)
                    return NotFound();

                using (s)
                using (var ms = new MemoryStream())
                {
                    // TODO: refactor this so we don't have to keep the whole
                    //       module content in memory and do more efficient streaming
                    s.CopyTo(ms);
                    var bytes = ms.ToArray();

                    using (var hash = SHA256.Create())
                    {
                        var csum = hash.ComputeHash(bytes);
                        var csumHex = System.BitConverter.ToString(csum).Replace("-", "");

                        base.Response.Headers.Add(
                                GetModuleResponse.ChecksumAlgorithmHeader, "SHA-256");
                        base.Response.Headers.Add(
                                GetModuleResponse.ChecksumHeader, csumHex);
                    }

                    return File(bytes, DscContentTypes.OCTET_STREAM);
                }
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("Modules(ModuleName='{ModuleName}',ModuleVersion='{ModuleVersion}')/ModuleContent")]
        public IActionResult GetModule(GetModuleRequest input)
        {
            if (ModelState.IsValid)
            {
                var s = _dscHandler.GetModule(input.ModuleName, input.ModuleVersion, input);
                if (s == null)
                    return NotFound();

                using (s)
                using (var ms = new MemoryStream())
                {
                    // TODO: refactor this so we don't have to keep the whole
                    //       module content in memory and do more efficient streaming
                    s.CopyTo(ms);
                    var bytes = ms.ToArray();

                    using (var hash = SHA256.Create())
                    {
                        var csum = hash.ComputeHash(bytes);
                        var csumHex = System.BitConverter.ToString(csum).Replace("-", "");

                        base.Response.Headers.Add(
                                GetModuleResponse.ChecksumAlgorithmHeader, "SHA-256");
                        base.Response.Headers.Add(
                                GetModuleResponse.ChecksumHeader, csumHex);
                    }

                    return File(bytes, DscContentTypes.OCTET_STREAM);
                }
            }

            return BadRequest(ModelState);
        }
    }
}