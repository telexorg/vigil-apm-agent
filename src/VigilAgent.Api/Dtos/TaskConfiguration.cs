using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VigilAgent.Api.Dtos
{
    public class TaskConfiguration
    {
        public PushNotificationConfig PushNotification { get; set; }

    }

    public class PushNotificationConfig
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public Authentication Authentication { get; set; }

    }

    public class Authentication
    {
        public string[]? Schemes { get; set; }
    }
}
