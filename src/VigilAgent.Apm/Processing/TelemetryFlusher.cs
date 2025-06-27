using Timer = System.Timers.Timer;
using Microsoft.Extensions.Logging;
using VigilAgent.Apm.Processing;
using VigilAgent.Apm.Telemetry;

namespace VigilAgent.Apm.Processing
{
    public class TelemetryFlusher : IAsyncDisposable, ITelemetryFlusher
    {
        private readonly ITelemetryClient _client;
        private readonly TelemetryBuffer _buffer;
        private readonly ILogger<TelemetryFlusher> _logger;
        private readonly Timer _flushTimer;

        public TelemetryFlusher(
            ITelemetryClient client,
            TelemetryBuffer buffer,
            ILogger<TelemetryFlusher> logger)
        {
            _client = client;
            _buffer = buffer;
            _logger = logger;

            _flushTimer = new Timer(5000); // configurable if needed
            _flushTimer.Elapsed += async (_, _) => await FlushAsync();
            _flushTimer.AutoReset = true;
        }

        public void Start()
        {
            _flushTimer.Start();
            _logger.LogInformation("[Vigil.Flusher] Background flusher started");
        }

        public async Task FlushAsync()
        {
                var batch = _buffer.DrainBatch();
                if (batch.Count == 0)
                {
                    _logger.LogDebug("[Vigil.Flusher] No events to flush");
                    return;
                }
            try
            {

                _logger.LogDebug("[Vigil.Flusher] Flushing {Count} events", batch.Count);
                var isSent = await _client.SendBatchAsync(batch);

                if (!isSent)
                {

                    _logger.LogError("[Vigil.Flusher] Flush failed—re-queueing {Count} events", batch.Count);

                    foreach (var evt in batch)
                    {
                        if (evt is TelemetryEvent typed && typed.RetryCount < 3)
                        {
                            typed.RetryCount++;
                            _buffer.Requeue(typed);
                        }
                        else
                        {
                            _logger.LogWarning("Dropped telemetry event after max retries: {Type}", evt?.GetType().Name ?? "unknown");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
                _logger.LogError(ex, "[Vigil.Flusher] Unexpected flush failure");

                foreach (var evt in batch)
                {
                    if (evt is TelemetryEvent typed && typed.RetryCount < 3)
                    {
                        typed.RetryCount++;
                        _buffer.Requeue(typed);
                    }
                    else
                    {
                        _logger.LogWarning("[Vigil.Flusher] Dropping event after max retries (exception): {Type}", evt?.GetType().Name ?? "unknown");
                    }
                }


            }

        }

        public async ValueTask DisposeAsync()
        {
            _flushTimer.Stop();
            _flushTimer.Dispose();

            _logger.LogInformation("[Vigil.Flusher] Disposing—flushing remaining events");
            await FlushAsync();
        }
    }
}