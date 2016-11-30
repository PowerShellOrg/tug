using System;
using System.ComponentModel.DataAnnotations;

namespace tug.Messages
{
    public class GetReportsRequest : DscAgentRequest
    {
        [Required]
        public Guid JobId
        { get; set; }
    }
}