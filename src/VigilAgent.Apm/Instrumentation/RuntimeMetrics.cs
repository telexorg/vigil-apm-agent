﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Instrumentation
{
    public class RuntimeMetrics : TelemetryEvent
    {
        public double CpuUsagePercent { get; set; }
        public long MemoryUsageBytes { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }

        // Delta (per interval)
        public int DeltaGen0 { get; set; }
        public int DeltaGen1 { get; set; }
        public int DeltaGen2 { get; set; }

        public int AvailableWorkerThreads { get; set; }
        public int AvailableIOThreads { get; set; }

    }
}
