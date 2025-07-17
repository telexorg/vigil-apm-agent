using VigilAgent.Api.Commons;
using VigilAgent.Api.Configuration;
using VigilAgent.Api.Contracts;
using VigilAgent.Api.Data;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Repositories;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Extension;
using VigilAgent.Apm.Middleware;

namespace VigilAgent.Api.Extension
{
    public static class DIServiceExtention
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<TelexApiSettings>(configuration.GetSection("TelexApiSettings"));
            services.Configure<MongoOptions>(configuration.GetSection("Mongo"));
            services.Configure<ApiSecret>(configuration.GetSection("ApiSecrets"));
            services.Configure<TelexApiSettings>(configuration.GetSection("TelexApiSettings"));
            services.AddTelemetryExporter(configuration);
            services.AddHttpContextAccessor();
            services.AddHttpClient(); 
            services.AddSingleton<MongoDbContext>();
            services.AddSingleton<TelemetryFunctions>();
            services.AddSingleton<IApiKeyManager, ApiKeyManager>();
            services.AddSingleton<ITaskStreamClient, TaskStreamClient>();
            services.AddSingleton<KernelProvider>();

            services.AddScoped<HttpHelper>();
            services.AddScoped<TelexDbContext>();
            services.AddScoped<ITelemetryService, TelemetryService>();            
            services.AddScoped(typeof(ITelexRepository<>), typeof(TelexRepository<>));
            services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
            services.AddScoped(typeof(ITelemetryRepository<>), typeof(TelemetryRepository<>));
            services.AddScoped<IVigilAgentService, VigilAgentService>();
            services.AddScoped<IAIService, AIService>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<ITelemetryHandler, TelemetryHandler>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITaskContextProvider, TaskContextProvider>();
            services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
            services.AddScoped<ITaskStreamHandler, TaskStreamHandler>();

            //services.AddTelemetryExporter(configuration);


            var configRoot = (IConfigurationRoot)configuration;

            foreach (var provider in configRoot.Providers)
            {
                Console.WriteLine($"📦 Loaded provider: {provider}");
            }


            return services;
        }
    }
}
