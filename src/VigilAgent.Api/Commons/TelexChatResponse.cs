using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Commons
{
    public class TelexChatResponse
    {
        public TelexChatMessage Messages { get; set; } = new();
    }
}
