using VigilAgent.Api.IServices;
using VigilAgent.Api.Middleware;
using VigilAgent.Api.Services;
using VigilAgent.Apm.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAgentCommandHandler, ShowLogsHandler>();
builder.Services.AddScoped<IAgentCommandHandler, ShowRuntimeMetrics>();
builder.Services.AddScoped<IAgentCommandHandler, ExplainErrorsHandler>();

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAnyOrigin");

app.UseMiddleware<ExceptionHandler>();

app.UseMiddleware<VigilMiddleware>();
//app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
