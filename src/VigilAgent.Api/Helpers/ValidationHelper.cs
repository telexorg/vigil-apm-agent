
using VigilAgent.Api.Dtos;

namespace VigilAgent.Api.Helpers
{
    public class ValidationHelper
    {
        public static void ValidateRequest(TaskRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Jsonrpc != "2.0")
                throw new ArgumentException("Invalid JSON-RPC version");

            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("Id is required");

            if (request.Method?.ToLower() != "message/send")
                throw new ArgumentException("Invalid method");

            var message = request.Params?.Message;
            if (message == null)
                throw new ArgumentException("Message params are required");

            if (message.Role?.ToLower() != "user")
                throw new ArgumentException("Role must be 'user'");

            if (message.Parts == null || !message.Parts.Any())
                throw new ArgumentException("Message parts cannot be empty");

            foreach (var part in message.Parts)
            {
                if (part.Kind != "text")
                    throw new ArgumentException("Only 'text' type supported in message parts");

                if (string.IsNullOrWhiteSpace(part.Text))
                    throw new ArgumentException("Text cannot be empty");
            }

            // Validate IDs are GUIDs (optional but recommended)
            if (!Guid.TryParse(message.ContextId, out _))
                throw new ArgumentException("Invalid ContextId format");

            if (message.TaskId != null && !Guid.TryParse(message.TaskId, out _))
                throw new ArgumentException("Invalid TaskId format");

            if (!Guid.TryParse(message.MessageId, out _))
                throw new ArgumentException("Invalid MessageId format");

            // Optional: Validate push notification config if present
            var config = request.Params.Configuration;
            if (config?.PushNotificationConfig != null)
            {
                if (string.IsNullOrWhiteSpace(config.PushNotificationConfig.Url))
                    throw new ArgumentException("PushNotification Url is required");

                if (string.IsNullOrWhiteSpace(config.PushNotificationConfig.Token))
                    throw new ArgumentException("PushNotification Token is required");
            }
        }

       
    }
}

