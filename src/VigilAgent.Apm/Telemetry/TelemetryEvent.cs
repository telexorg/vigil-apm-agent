using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Telemetry
{
    public class TelemetryEvent
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int RetryCount { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

