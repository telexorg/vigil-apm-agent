using VigilAgent.Api.IServices;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.Models;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.Data;
using VigilAgent.Api.Enums;
using VigilAgent.Api.Commons;

namespace VigilAgent.Api.Services
{
    public class RequestProcessingService : IRequestProcessingService
    {
        private static readonly HashSet<string> ErrorKeywords = new()
        { };

        private static readonly HashSet<string> TraceKeywords = new()
        { };

        private static readonly HashSet<string> MetricKeywords = new()
        { };

        private readonly ITelexRepository<Organization> _companyRepository;

        public RequestProcessingService(ITelexRepository<Organization> companyRepository)
        {
            _companyRepository = companyRepository;
        }
       

        public async Task<Request> ProcessUserInputAsync(TelemetryTask taskRequest)
        {
            string companyId = taskRequest.MessageId;
            string userId = taskRequest.ContextId;
                       
            var company = new Document<Organization>(){ Data = new Organization()};                 

            var userInput = taskRequest.Message;

            // Classify request (e.g., fetch blog, summarize, generate, etc.)
            var classification = ClassifyRequest(userInput);

            var prompt = userInput;
            var systemMessage = GenerateSystemMessage(classification, taskRequest.Settings);

            // Generate appropriate prompt based on classification
            if (classification == RequestType.ErrorRequst)
            {
                prompt = GeneratePrompt(userInput, taskRequest.Settings, company.Data);
            }
           
           
            // Step 4: Return structured response
            return new Request
            {
                SystemMessage = systemMessage,
                UserPrompt = prompt
            };
        }
               

        private string GeneratePrompt(string userPrompt, List<Setting> settings, Organization company)
        {
            

            // Base prompt structure
            string prompt = $"{userPrompt}.";    

           

            return prompt;
        }

        private string GenerateSystemMessage(RequestType requestType, List<Setting> settings)
        {
            string systemMessage = "Your name is Vigil. You are an APM agent who specializes in helping developers monitor there application providing insights for debugging and improving performance.";

           

            return systemMessage;
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

        public string GetBlogIntervalOption(TelemetryTask blogDto)
        {
            // Retrieve settings dynamically
            string blogInterval = DataExtract.GetSettingValue(blogDto.Settings, "blog_generation_interval");

            return blogInterval;
        }
      
    }
}

