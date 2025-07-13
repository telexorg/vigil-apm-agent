using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VigilAgent.Apm.Instrumentation;
using VigilAgent.Apm.Middleware;
using VigilAgent.Apm.Processing;

namespace VigilAgent.Apm.Extension
{
    public static class VigilMiddlewareExtensions
    {
        public static IApplicationBuilder UseVigilTelemetry(this IApplicationBuilder app)
        {
            // 1. Register the middleware
            app.UseMiddleware<VigilMiddleware>();

            // 2. Bootstrap background agents (flushers, etc.)
            StartBackgroundServices(app.ApplicationServices);

            return app;

        }

        private static void StartBackgroundServices(IServiceProvider serviceProvider)
        {
            // Use a scope in case background services need scoped dependencies
            
            var flusher = serviceProvider.GetRequiredService<ITelemetryFlusher>();
            var metrics = serviceProvider.GetRequiredService<MetricsCollector>();
                
               metrics.Start();

            flusher.Start();
        }


    }
}
