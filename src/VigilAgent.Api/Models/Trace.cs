using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Models
{
    public class Trace : ITelemetryItem
    {
        public string  Id { get; set; }
        public string Type { get; set; } 
        public string Path { get; set; }
        public string  Method { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool isError { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string OrgId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
