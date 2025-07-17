using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Dtos
{
    public class TaskRequest
    {
        public string Jsonrpc { get; set; }
        public string Id { get; set; }
        public string Method { get; set; }
        public TaskParams Params { get; set; }
       
        public static object SendTaskRequest()
        {
            return new TaskRequest
            {
                Jsonrpc = "2.0",
                Id = "c006266b7e954f2fb07eb02b26ce6d9e",
                Method = "meesage/send",
                Params = new TaskParams
                {

                    Message = new TaskMessage
                    {
                        Kind = "message",
                        Role = "user",
                        Parts = new List<MessagePart>
                        {
                            new MessagePart
                            {
                                Kind = "text",
                                Text = "Hello",
                                Metadata = null
                            }
                        },
                        Metadata = null,
                        TaskId = "0195c514-2292-71e2-9378-21d11be2ad8c",
                        ContextId = "0195c514-2292-71e2-9378-21d11be2ad8c",
                        MessageId = "0195c514-2292-71e2-9378-21d11be2ad8c"
                    },
                    Configuration = null,
                    Metadata = null
                }
            };

        }
    }


}
