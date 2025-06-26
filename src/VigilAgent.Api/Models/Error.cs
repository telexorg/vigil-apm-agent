using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Models
{
    public class Error : ITelemetryItem
    {
        public string TraceId { get; set; }            
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "error";
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string OrgId { get; set; }
        public long OccurredAt { get; set; }

        public string HttpMethod { get; set; }
        public string Url { get; set; }                
        public int StatusCode { get; set; }

        public string Source { get; set; }             
        public string Application { get; set; }        
    }
}
