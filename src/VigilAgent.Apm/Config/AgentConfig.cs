using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Config
{
   
        public class AgentConfig
        {
            public int FlushIntervalMs { get; set; } = 5000;
            public int MaxBatchSize { get; set; } = 10;
            public string TelemetryEndpoint { get; set; } = "https://telex.io/api/telemetry";
            public string ApiKey { get; set; }
            public string ServiceName { get; set; } = "default-service";
            public string Environment { get; set; } = "development";
            public bool EnableRuntimeMetrics { get; set; } = true;
            public bool EnableExceptionLogging { get; set; } = true;
            public bool EnableDebugLogs { get; set; } = false;
        }
 

}
