using VigilAgent.Api.Commons;
using VigilAgent.Api.Data;
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
builder.Services.AddSingleton<KernelProvider>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAgentCommandHandler, ShowLogsHandler>();
builder.Services.AddScoped<IAgentCommandHandler, ShowRuntimeMetrics>();
builder.Services.AddScoped<IAgentCommandHandler, ExplainErrorsHandler>();
builder.Services.AddScoped<IVigilAgentService, VigilAgentService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IRequestProcessingService, RequestProcessingService>();
builder.Services.AddScoped(typeof(ITelexRepository<>), typeof(TelexRepository<>));
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();


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

app.UseMiddleware<RequestLoggingMiddleware>();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
