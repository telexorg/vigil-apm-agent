using VigilAgent.Api.Configuration;
using VigilAgent.Api.IServices;

namespace VigilAgent.Api.Infrastructure
{
    public static class AppBootstrapper
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var kernelProvider = scope.ServiceProvider.GetRequiredService<KernelProvider>();
                kernelProvider.RegisterPlugins(scope.ServiceProvider);
            }

            using (var scope = services.CreateScope())
            {
                var telemetryHandler = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
                await telemetryHandler.EnsureWarmCacheAsync();
            }
        }
    }
}
