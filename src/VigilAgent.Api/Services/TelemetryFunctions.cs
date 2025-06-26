using Microsoft.SemanticKernel;
using System.ComponentModel;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Services
{
    public class TelemetryFunctions
    {
       
        private readonly IServiceScopeFactory _sp;

        public TelemetryFunctions(IServiceScopeFactory sp)
        {
            _sp = sp;           
        }


        [KernelFunction("get_current_date_and_time")]
        public string getDate()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        } 

        [KernelFunction("get_request_traces"), Description("Returns system recent trace logs. Use this to retrieve users request traces")]
        public async Task<string> GetLogsAsync(string projectName, string timeRange)
        {
            using var scope = _sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
            return await handler.GetLogs(projectName, timeRange);
        }

        [KernelFunction("get_system_errors"), Description("Returns system recent errors. Use this to retrieve users system errors for them")]
        public async Task<string> GetErrorsAsync(string projectName, string timeRange)
        {
            using var scope = _sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
            return await handler.GetErrors(projectName, timeRange);
        }

        [KernelFunction("get_system_metrics"), Description("Returns system recent metrics, Use this to retrieve users system metrics for them")]
        public async Task<string> GetMetricsAsync(string projectName, string timeRange)
        {
            using var scope = _sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
            return await handler.GetMetrics(projectName,timeRange);
        }

    }
}
