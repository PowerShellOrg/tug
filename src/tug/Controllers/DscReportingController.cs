using Microsoft.AspNetCore.Mvc;
using tug.Messages;

namespace tug.Controllers
{
    public class DscReportingController : Controller
    {
        [HttpPost]
        [Route("Nodes(AgentID='{AgentId}')/SendReport")]
        public IActionResult SendReport(SendReportRequest input)
        {
            if (ModelState.IsValid)
            {
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
