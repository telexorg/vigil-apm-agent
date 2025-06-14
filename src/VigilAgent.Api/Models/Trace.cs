using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Models
{
    public class Trace
    {
        public string  TraceId { get; set; }
        public string Type { get; set; } 
        public string Path { get; set; }
        public string  Method { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
