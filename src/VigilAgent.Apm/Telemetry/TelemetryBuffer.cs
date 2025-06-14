using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Telemetry
{
    public class TelemetryBuffer
    {
        private static readonly ConcurrentQueue<object> _buffer = new();
        private const int MaxBatchSize = 10;


        public static void Add(object evt)
        {
            _buffer.Enqueue(evt);

                if (_buffer.Count >= MaxBatchSize)
                TelemetryFlusher.Flush();
        }

        public static List<object> DrainBatch()
        {
            var batch = new List<object>();
            while (batch.Count < MaxBatchSize && _buffer.TryDequeue(out var evt))
            {
                batch.Add(evt);
            }

            return batch;
        }
    }
}
