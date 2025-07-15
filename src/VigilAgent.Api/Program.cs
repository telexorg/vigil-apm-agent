using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using VigilAgent.Api.Commons;
using VigilAgent.Api.Configuration;
using VigilAgent.Api.Data;
using VigilAgent.Api.Extension;
using VigilAgent.Api.Infrastructure;
using VigilAgent.Api.IRepositories;
using VigilAgent.Api.IServices;
using VigilAgent.Api.Middleware;
using VigilAgent.Api.Repositories;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Extension;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDIServices(builder.Configuration);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
builder.Services.AddApplicationInsightsTelemetry();


var app = builder.Build();


await AppBootstrapper.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");

app.UseMiddleware<ExceptionHandler>();
app.UseMiddleware<RequestHandler>();

app.UseVigilTelemetryCollector();

app.UseWhen(ctx =>
    ctx.Request.Path.StartsWithSegments(
        "/api/v1/Telemetry", 
        StringComparison.OrdinalIgnoreCase) && ctx.Request.Method == "POST",
    appBuilder => appBuilder.UseMiddleware<VigilAuthorizationMiddleware>()
    );

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
