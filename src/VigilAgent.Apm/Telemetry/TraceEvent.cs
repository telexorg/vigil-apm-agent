using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Telemetry
{
    public class TraceEvent : TelemetryEvent
    {
        public string Path { get; set; }
        public string  Method { get; set; }
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public bool isError { get; set; } = false;
    }
}
