using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Telemetry
{
    public class ErrorDetail
    {
        public string TraceId { get; set; }             // Link to the trace
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Unique ID for the error
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "error";
        public string ExceptionType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerExceptionMessage { get; set; }

        public long OccurredAt { get; set; }

        public string HttpMethod { get; set; }
        public string Url { get; set; }                
        public int StatusCode { get; set; }

        public string Source { get; set; }              // Optional: where the error occurred 
        public string Application { get; set; }         // Optional: For multi-app tracking
    }
}
