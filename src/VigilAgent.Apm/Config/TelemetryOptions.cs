using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Config
{
   
   public class TelemetryOptions
   {
       public const string TelemetryEndpoint  = "https://vigil-apm-agent.onrender.com/api/v1/Telemetry";
       public int FlushIntervalMs { get; set; } = 5000;
       public int MaxBatchSize { get; set; } = 10;
       public string ApiKey { get; set; }
       public string ProjectName { get; set; } = "backend-service";
       public string Environment { get; set; } = "development";
       public bool EnableRuntimeMetrics { get; set; } = true;
       public bool EnableExceptionLogging { get; set; } = true;
       public bool EnableDebugLogs { get; set; } = false;
       public string EndpointDevelopment { get; set; }
       public string EndpointStaging { get; set; }
       public string EndpointProduction { get; set; }
       public int MaxRetryCount { get; set; } = 3;
       public bool EnableConsoleLogging { get; set; } = false;
       public Dictionary<string, string>? DefaultTags { get; set; }
    } 

}




