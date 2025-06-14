using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Models
{
    public class Error
    {
        public string TraceId { get; set; }            
        public string ErrorId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "error";
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionMessage { get; set; }

        public long OccurredAt { get; set; }

        public string HttpMethod { get; set; }
        public string Url { get; set; }                
        public int StatusCode { get; set; }

        public string Source { get; set; }             
        public string Application { get; set; }        
    }
}
