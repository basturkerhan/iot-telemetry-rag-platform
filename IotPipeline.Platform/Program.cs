using FluentValidation;
using IotPipeline.Platform.Common.Configurations;
using IotPipeline.Platform.Features.API.Analytics.Hubs;
using IotPipeline.Platform.Features.API.Shared.Behaviors;
using IotPipeline.Platform.Features.API.Shared.Handlers;
using IotPipeline.Platform.Features.API.Shared.Modules;
using IotPipeline.Platform.Features.Bridge.Services;
using IotPipeline.Platform.Infrastructure;
using IotPipeline.Platform.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

#region Options Pattern
services.Configure<MqttSettings>(configuration.GetSection("MqttSettings"));
services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQSettings"));
#endregion

#region Background Workers
services.AddHostedService<MqttListenerService>();
#endregion

#region MassTransit
services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);

    x.AddMediator(cfg =>
    {
        cfg.AddConsumers(typeof(Program).Assembly);
        cfg.ConfigureMediator((context, m) =>
        {
            m.UseConsumeFilter(typeof(ValidationFilter<>), context);
        });
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var settings = context.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
        cfg.Host(settings.Host, "/", h =>
        {
            h.Username(settings.Username);
            h.Password(settings.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});
#endregion

#region FluentValidation
services.AddValidatorsFromAssemblyContaining<Program>();
#endregion

#region Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
#endregion

#region HealthCheck
var pgConnection = configuration.GetConnectionString("DefaultConnection");
var rabbitSettings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();
string rabbitConnectionString = $"amqp://{rabbitSettings?.Username}:{rabbitSettings?.Password}@{rabbitSettings?.Host}:{rabbitSettings?.Port}";

services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    Uri = new Uri(rabbitConnectionString)
});

services.AddHealthChecks()
    .AddNpgSql(pgConnection!, name: "Database")
    .AddRabbitMQ(name: "Queue")
    .AddCheck<MqttHealthCheck>("MQTT");
#endregion

#region CORS
var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
#endregion

services
    .AddInfrastructure(configuration)
    .AddOpenApi()
    .AddSignalR();

var app = builder.Build();

app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = $"{report.TotalDuration.TotalMilliseconds} ms",
            Services = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy ? "Online" : "Offline",
                Description = e.Value.Description ?? e.Value.Exception?.Message,
                Duration = $"{e.Value.Duration.TotalMilliseconds} ms"
            })
        };

        context.Response.StatusCode = report.Status == Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapProjectModules();

app.MapHub<AnalyticsHub>("/analytics-hub");

app.Run();
