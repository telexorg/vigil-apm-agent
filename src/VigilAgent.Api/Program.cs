using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
using VigilAgent.Api.Helpers;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Middleware;
using VigilAgent.Api.Repositories;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<TelexApiSettings>(builder.Configuration.GetSection("TelexApiSettings"));

builder.Services.AddScoped<HttpHelper>();
builder.Services.AddScoped<TelexDbContext>();
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddSingleton<KernelProvider>();

builder.Services.AddHttpClient();
//builder.Services.AddScoped<ITelemetryHandler, TelemetryHandler>();
builder.Services.AddScoped<IVigilAgentService, VigilAgentService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped(typeof(ITelexRepository<>), typeof(TelexRepository<>));
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.AddScoped(typeof(ITelemetryRepository<>), typeof(TelemetryRepository<>));
builder.Services.AddScoped<ITelemetryHandler, TelemetryHandler>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITaskContextProvider, TaskContextProvider>();
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddHttpContextAccessor();

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<TelemetryFunctions>();

ApiKeyManager.Configure(builder.Configuration);
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

var configRoot = (IConfigurationRoot)builder.Configuration;

foreach (var provider in configRoot.Providers)
{
    Console.WriteLine($"📦 Loaded provider: {provider}");
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // SAFE plugin registration here
    var kernelProvider = scope.ServiceProvider.GetRequiredService<KernelProvider>();
    kernelProvider.RegisterPlugins(scope.ServiceProvider);
}

using (var scope = app.Services.CreateScope())
{
    var telemetryHandler = scope.ServiceProvider.GetRequiredService<ITelemetryService>();
    await telemetryHandler.EnsureWarmCacheAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");

app.UseMiddleware<ExceptionHandler>();

app.UseMiddleware<VigilMiddleware>();
//app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<RequestHandler>();

app.UseWhen(ctx =>
    ctx.Request.Path.StartsWithSegments("/api/v1/Telemetry", StringComparison.OrdinalIgnoreCase) && ctx.Request.Method == "POST",
    appBuilder => appBuilder.UseMiddleware<ApiKeyAuthMiddleware>());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
