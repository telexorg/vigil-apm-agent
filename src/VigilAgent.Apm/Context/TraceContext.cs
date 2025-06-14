using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Apm.Context
{
    public class TraceContext
    {
        private static readonly AsyncLocal<string?> _traceId = new();

        public static string? TraceId
        {
            get => _traceId.Value;
            set => _traceId.Value = value;
        }

        public static void Clear() => _traceId.Value = null;
    }
}
