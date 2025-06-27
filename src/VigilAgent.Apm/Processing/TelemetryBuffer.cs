using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Processing
{
    public class TelemetryBuffer
    {
        private readonly ILogger<TelemetryBuffer> _logger;
        private static readonly ConcurrentQueue<object> _buffer = new();
        private const int MaxBatchSize = 10;
        private const int MaxQueueSize = 1000;

        public TelemetryBuffer(ILogger<TelemetryBuffer> logger)
        {
            _logger = logger;
        }

        public int Count => _buffer.Count;

        public async Task AddAsync(object evt)
        {
            if (_buffer.Count >= MaxQueueSize)
            {
                _logger.LogWarning("Telemetry buffer full ({Count}/{Limit}). Dropping event of type {EventType}",
                    _buffer.Count, MaxQueueSize, evt?.GetType().Name ?? "unknown");

                return;
            }

            _buffer.Enqueue(evt);
            _logger.LogDebug("Telemetry event enqueued: {Type}", evt?.GetType().Name ?? "unknown");
           
        }

        public void Requeue(object evt)
        {
            _logger.LogDebug("Re-queued telemetry event of type {Type} (RetryCount={Count})",
                evt?.GetType().Name ?? "unknown",
                (evt as TelemetryEvent)?.RetryCount ?? -1);

            _buffer.Enqueue(evt);
        }

        public List<object> DrainBatch()
        {
            var batch = new List<object>(MaxBatchSize);

            while (batch.Count < MaxBatchSize && _buffer.TryDequeue(out var evt))
            {
                batch.Add(evt);
            }

            _logger.LogDebug("Drained {Count} telemetry events", batch.Count);
            return batch;
        }
    }
}