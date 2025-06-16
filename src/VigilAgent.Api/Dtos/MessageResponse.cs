using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Dtos
{
    public class MessageResponse
    {
        public string Jsonrpc { get; set; }
        public string Id { get; set; }
        public ResponseMessage Result { get; set; }
    }
}
