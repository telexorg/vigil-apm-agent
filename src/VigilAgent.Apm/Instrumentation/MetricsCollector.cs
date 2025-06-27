using System.Diagnostics;
using Timer = System.Timers.Timer;
using Microsoft.Extensions.Logging;
using VigilAgent.Apm.Processing;

namespace VigilAgent.Apm.Instrumentation
{
    public class MetricsCollector
    {
        private readonly ILogger<MetricsCollector> _logger;
        private readonly TelemetryBuffer _telemetryBuffer;
        private readonly Timer _metricsTimer;
        private readonly Process _process = Process.GetCurrentProcess();

        private DateTimeOffset _lastCheck = DateTimeOffset.UtcNow;
        private TimeSpan _lastCpu = Process.GetCurrentProcess().TotalProcessorTime;

        private int _lastGen0 = GC.CollectionCount(0);
        private int _lastGen1 = GC.CollectionCount(1);
        private int _lastGen2 = GC.CollectionCount(2);

        public MetricsCollector(ILogger<MetricsCollector> logger, TelemetryBuffer telemetryBuffer)
        {
            _logger = logger;
            _metricsTimer = new Timer(600_000); // every 10 min
            _metricsTimer.Elapsed += (_, _) => SafeCollect();
            _metricsTimer.AutoReset = true;
            _telemetryBuffer = telemetryBuffer;
        }

        public void Start()
        {
            _logger.LogInformation("[Vigil.Metrics] Collector started");
            _metricsTimer.Start();
        }

        public void Stop()
        {
            _metricsTimer.Stop();
            _logger.LogInformation("[Vigil.Metrics] Collector stopped");
        }

        public async void Collect()
        {
            _process.Refresh();
            ThreadPool.GetAvailableThreads(out int worker, out int io);

            var currentGen0 = GC.CollectionCount(0);
            var currentGen1 = GC.CollectionCount(1);
            var currentGen2 = GC.CollectionCount(2);

            var deltaGen0 = currentGen0 - _lastGen0;
            var deltaGen1 = currentGen1 - _lastGen1;
            var deltaGen2 = currentGen2 - _lastGen2;

            _lastGen0 = currentGen0;
            _lastGen1 = currentGen1;
            _lastGen2 = currentGen2;

            var cpu = GetCpuUsage();
            var mem = _process.WorkingSet64;

            var metrics = new RuntimeMetrics
            {
                Id = Guid.NewGuid().ToString(),
                Type = "metrics",
                CpuUsagePercent = cpu,
                MemoryUsageBytes = mem,
                Gen0Collections = currentGen0,
                Gen1Collections = currentGen1,
                Gen2Collections = currentGen2,
                DeltaGen0 = deltaGen0,
                DeltaGen1 = deltaGen1,
                DeltaGen2 = deltaGen2,
                AvailableWorkerThreads = worker,
                AvailableIOThreads = io,
            };

            await _telemetryBuffer.AddAsync(metrics);

            _logger.LogInformation(
                "[Metrics] {Timestamp} | CPU: {CPU}% | Mem: {Mem}MB | GC Δ: Gen0={Gen0}, Gen1={Gen1}, Gen2={Gen2} | Threads: Worker={Worker}, IO={IO}",
                metrics.Timestamp.TimeOfDay,
                cpu,
                mem / 1024 / 1024,
                deltaGen0,
                deltaGen1,
                deltaGen2,
                worker,
                io
            );
        }

        private double GetCpuUsage()
        {
            _process.Refresh();

            var now = DateTimeOffset.UtcNow;
            var cpuNow = _process.TotalProcessorTime;

            var cpuDelta = (cpuNow - _lastCpu).TotalMilliseconds;
            var wallTime = (now - _lastCheck).TotalMilliseconds;

            _lastCpu = cpuNow;
            _lastCheck = now;

            return Math.Round((cpuDelta / (wallTime * Environment.ProcessorCount)) * 100, 2);
        }

        private void SafeCollect()
        {
            try
            {
                Collect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Vigil.Metrics] Failed during collection");
            }
        }
    }
}