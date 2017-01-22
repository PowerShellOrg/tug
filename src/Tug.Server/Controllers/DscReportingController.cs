/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System.Linq;
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
        private IDscHandler _dscHandler;

        public DscReportingController(ILogger<DscController> logger,
                DscHandlerHelper dscHelper)
        {
            _logger = logger;
            _dscHelper = dscHelper;
            _dscHandler = _dscHelper.DefaultHandler;
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
                _dscHandler.SendReport(input.AgentId.Value, input.Body);
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route(GetReportsRequest.ROUTE_SINGLE,
            Name = GetReportsRequest.ROUTE_SINGLE_NAME)]
        [ActionName(nameof(GetReportsSingle))]
        public IActionResult GetReportsSingle(GetReportsRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetReportsSingle)}:  {GetReportsRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");
                var sr = _dscHandler.GetReports(input.AgentId.Value, input.JobId);
                
                return this.Model(new GetReportsSingleResponse
                {
                    Body = sr.FirstOrDefault(),
                });
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route(GetReportsRequest.ROUTE_ALL,
            Name = GetReportsRequest.ROUTE_ALL_ALT_NAME)]
        [Route(GetReportsRequest.ROUTE_ALL,
            Name = GetReportsRequest.ROUTE_ALL_ALT_NAME)]
        [ActionName(nameof(GetReportsAll))]
        public IActionResult GetReportsAll(GetReportsRequest input)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"{nameof(GetReportsAll)}:  {GetReportsRequest.VERB}");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");
                var sr = _dscHandler.GetReports(input.AgentId.Value, null);
                
                return this.Model(new GetReportsAllResponse
                {
                    Body = new Model.GetReportsAllResponseBody
                    {
                        Value = sr,
                    },
                });
            }

            return BadRequest(ModelState);
        }
    }
}
