/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tug.Messages;

namespace Tug.Server.Controllers
{
    public class DscReportingController : Controller
    {
        private ILogger<DscController> _logger;
        private IDscHandlerProvider _dscHandlerProvider;
        public DscReportingController(ILogger<DscController> logger,
                IDscHandlerProvider handlerProvider)
        {
            _logger = logger;
            _dscHandlerProvider = handlerProvider;
        }

        [HttpPost]
        [Route("Nodes(AgentID='{AgentId}')/SendReport")]
        public IActionResult SendReport(SendReportRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: Report delivery");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                // TODO:
                // persist the report content indexed by the JobId
                var jobId = input.Body.JobId;
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("Nodes(AgentId='{AgentId}')/Reports(JobId='{JobId}'))")]
        public IActionResult GetReports(GetReportsRequest input)
        {
            if (ModelState.IsValid)
            {
                return Json(new
                {
                    JobId = input.JobId,
                    Message = "Argh!  Here be the report data!",
                });
            }

            return BadRequest(ModelState);
        }
    }
}
