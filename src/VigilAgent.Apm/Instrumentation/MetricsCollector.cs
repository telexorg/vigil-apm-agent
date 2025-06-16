using System.Diagnostics;
using Timer = System.Timers.Timer;

namespace VigilAgent.Apm.Instrumentation
{
    public class MetricsCollector
    {
        public static readonly Timer _metricsTimer;
        private static readonly Process _currentProcess = Process.GetCurrentProcess();
        private static DateTime _lastCheckTime = DateTime.UtcNow;
        private static TimeSpan _lastCpuTime = _currentProcess.TotalProcessorTime;

        // Previous GC counts
        private static int _lastGen0 = GC.CollectionCount(0);
        private static int _lastGen1 = GC.CollectionCount(1);
        private static int _lastGen2 = GC.CollectionCount(2);

        static MetricsCollector()
        {
            _metricsTimer = new Timer(600000);
            _metricsTimer.Elapsed += (_, _) => Collect();
            _metricsTimer.AutoReset = true;
        }

        public static void Start() => _metricsTimer.Start();

        public static void Collect()
        {
            _currentProcess.Refresh();

            // Thread pool info
            ThreadPool.GetAvailableThreads(out int workerThreads, out int ioThreads);

            // Current GC counts
            int currentGen0 = GC.CollectionCount(0);
            int currentGen1 = GC.CollectionCount(1);
            int currentGen2 = GC.CollectionCount(2);

            // Delta since last check
            int deltaGen0 = currentGen0 - _lastGen0;
            int deltaGen1 = currentGen1 - _lastGen1;
            int deltaGen2 = currentGen2 - _lastGen2;

            // Update previous counts
            _lastGen0 = currentGen0;
            _lastGen1 = currentGen1;
            _lastGen2 = currentGen2;

            var metrics = new RuntimeMetrics
            {
                CpuUsagePercent = GetCpuUsage(),
                MemoryUsageBytes = _currentProcess.WorkingSet64,
                Gen0Collections = currentGen0,
                Gen1Collections = currentGen1,
                Gen2Collections = currentGen2,
                DeltaGen0 = deltaGen0,
                DeltaGen1 = deltaGen1,
                DeltaGen2 = deltaGen2,
                AvailableWorkerThreads = workerThreads,
                AvailableIOThreads = ioThreads,
            };

            Telemetry.TelemetryBuffer.Add(metrics);

            Console.WriteLine($"""

            [Metrics]
              🕒 Timestamp     : {DateTime.Now}
              🔁 CPU Usage     : {metrics.CpuUsagePercent}%
              💾 Memory Usage  : {metrics.MemoryUsageBytes / 1024 / 1024}MB
              ♻️ GC Total      : Gen0={metrics.Gen0Collections}, Gen1={metrics.Gen1Collections}, Gen2={metrics.Gen2Collections}
              🧮 GC Deltas     : ΔGen0={metrics.DeltaGen0}, ΔGen1={metrics.DeltaGen1}, ΔGen2={metrics.DeltaGen2}
              🧵 Threads       : Worker={metrics.AvailableWorkerThreads}, IO={metrics.AvailableIOThreads}

            """);
        }

        // Approximate CPU usage (per interval, could be improved)
        private static double GetCpuUsage()
        {
            _currentProcess.Refresh();

            var currentCpuTime = _currentProcess.TotalProcessorTime;
            var currentTime = DateTime.UtcNow;

            var cpuUsedMs = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
            var elapsedMs = (currentTime - _lastCheckTime).TotalMilliseconds;

            _lastCpuTime = currentCpuTime;
            _lastCheckTime = currentTime;

            int processorCount = Environment.ProcessorCount;

            return Math.Round((cpuUsedMs / (elapsedMs * processorCount)) * 100, 2);
        }
    }
}
