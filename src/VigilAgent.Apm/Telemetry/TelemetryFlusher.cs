using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Middleware;
using VigilAgent.Apm.Transport;
using Timer = System.Timers.Timer;

namespace VigilAgent.Apm.Telemetry
{
    public class TelemetryFlusher
    {
        private static readonly Timer _flushTimer;

        static TelemetryFlusher()
        {
            _flushTimer = new Timer(5000);
            _flushTimer.Elapsed += (_, _) => Flush();
            _flushTimer.AutoReset = true;
        }

        public static void Start() => _flushTimer.Start();
        

        public static async void Flush()
        {
            var batch = TelemetryBuffer.DrainBatch();
            if (batch.Count == 0) return;

            await TelemetrySender.ExportAsync(batch);

            /*//foreach (var item in batch)
            //{
            //    switch (item)
            //    {
            //        case TelemetryEvent evt:
            //            Console.WriteLine($"[Batch] {evt.TraceId} trace - {evt.Path} = {evt.StatusCode} in {evt.DurationMs}ms");
            //            break;

            //        case ErrorDetail error:
            //            Console.WriteLine($"[Batch] {error.TraceId} exception - {error.Url} = {error.StatusCode} at {error.Timestamp}");
            //            Console.WriteLine($"          ✖ {error.ExceptionType}: {error.Message}");
            //            break;

            //        case RuntimeMetrics metrics:
            //            Console.WriteLine($"[Batch] metrics - CPU: {metrics.CpuUsagePercent}%, Mem: {metrics.MemoryUsageBytes / 1024 / 1024}MB, GC0: {metrics.Gen0Collections}");
            //            break;

            //    }
            //}*/
        }

    }
}
