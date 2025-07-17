using Microsoft.Extensions.DependencyInjection;
using VigilAgent.Apm.Config;
using VigilAgent.Apm.Processing;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.Options;
using VigilAgent.Apm.Processing;
using VigilAgent.Apm.Instrumentation;

namespace VigilAgent.Apm.Extension
{
    public static class VigilExporterServiceExtension
    {
        
        public static IServiceCollection AddTelemetryExporter(this IServiceCollection services, Action<TelemetryOptions> configureOptions)
        {
            services.Configure(configureOptions); // registers via IOptions<T>
            services.AddSingleton<ITelemetryFlusher,TelemetryFlusher>();
            services.AddHttpClient<ITelemetryClient, TelemetryClient>(); // your custom class
            services.AddSingleton<MetricsCollector>();
            services.AddSingleton<TelemetryBuffer>();
            return services;
        }

        public static IServiceCollection AddTelemetryExporter(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelemetryOptions>(configuration.GetSection("Vigil"));
            services.AddHttpClient<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ITelemetryFlusher, TelemetryFlusher>();
            services.AddSingleton<MetricsCollector>();
            services.AddSingleton<TelemetryBuffer>();
            return services;
        }

        public static IServiceCollection AddTelemetryExporter(this IServiceCollection services, IConfiguration configuration, Action<TelemetryOptions>? overrideOptions = null)
        {
            // Bind from config
            var telemetryOptions = new TelemetryOptions();
            configuration.GetSection("Vigil").Bind(telemetryOptions);

            // Apply override if present
            overrideOptions?.Invoke(telemetryOptions);

            // Validate critical fields
            if (string.IsNullOrWhiteSpace(telemetryOptions.ApiKey))
                throw new InvalidOperationException("Vigil TelemetryOptions.ApiKey is required.");

            // Register options as singleton
            services.AddSingleton(telemetryOptions);
            services.AddHttpClient<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ITelemetryFlusher,TelemetryFlusher>();
            services.AddSingleton<MetricsCollector>();
            services.AddSingleton<TelemetryBuffer>();

            return services;
        }
    }

}

