/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tug.Messages;
using Tug.Server.Util;

namespace Tug.Server.Controllers
{
    public class DscReportingController : Controller
    {
        private ILogger<DscController> _logger;
        private DscHandlerHelper _dscHelper;

        public DscReportingController(ILogger<DscController> logger,
                DscHandlerHelper dscHelper)
        {
            _logger = logger;
            _dscHelper = dscHelper;
        }

        [HttpPost]
        [Route(SendReportRequest.ROUTE,
            Name = SendReportRequest.ROUTE_NAME)]
        [ActionName(nameof(SendReport))]
        public IActionResult SendReport(SendReportRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(SendReport)}:  {SendReportRequest.VERB}");

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
        [Route(GetReportsRequest.ROUTE,
            Name = GetReportsRequest.ROUTE_NAME)]
        [ActionName(nameof(GetReports))]
        public IActionResult GetReports(GetReportsRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetReports)}:  {GetReportsRequest.VERB}");

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
