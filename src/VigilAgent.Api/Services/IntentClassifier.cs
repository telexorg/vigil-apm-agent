using VigilAgent.Api.IServices;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Data;
using VigilAgent.Api.Enums;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Services
{
    public class IntentClassifier : IIntentClassifier
    {
        private static readonly HashSet<string> ErrorKeywords = new()
        { };

        private static readonly HashSet<string> TraceKeywords = new()
        { };

        private static readonly HashSet<string> MetricKeywords = new()
        { };

        private readonly ITelexRepository<Organization> _companyRepository;

        public IntentClassifier(ITelexRepository<Organization> companyRepository)
        {
            _companyRepository = companyRepository;
        }       

        public async Task<Request> ProcessUserInputAsync(TelemetryTask taskRequest)
        {
            string companyId = taskRequest.MessageId;
            string userId = taskRequest.ContextId;
                       
            var company = new Document<Organization>(){ Data = new Organization()};                 

            var userInput = taskRequest.Message;

            
            var classification = ClassifyRequest(userInput);

            var prompt = userInput;
            var systemMessage = PromptBuilder.GenerateSystemMessage(classification, taskRequest.Settings);

           
            if (classification == RequestType.ErrorRequst)
            {
                //prompt = GeneratePrompt(userInput, taskRequest.Settings, company.Data);
            }           
           
            
            return new Request
            {
                SystemMessage = systemMessage,
                UserPrompt = prompt
            };
        }             

       

        public static RequestType ClassifyRequest(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return RequestType.Uncertain;
            }

            // Normalize input: convert to lowercase and split into words
            var words = message.ToLower().Split(new[] { ' ', '.', ',', '!', '?', '-', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);

            int errorScore = words.Count(word => ErrorKeywords.Contains(word));
            int traceScore = words.Count(word => TraceKeywords.Contains(word));
            int metricScore = words.Count(word => MetricKeywords.Contains(word));

            // Determine classification based on highest score
            if (metricScore > errorScore && metricScore > traceScore)
            {
                return RequestType.MetricRequest;
            }
            else if (errorScore >= traceScore && errorScore > 1)
            {
                return RequestType.ErrorRequst;
            }
            else if (traceScore > errorScore)
            {
                return RequestType.TraceRequest;
            }

            return RequestType.Uncertain;
        }

      
    }
}

