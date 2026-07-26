using Carter;
using CollectManagement.Application;
using CollectManagement.Application.Handlers;
using CollectManagement.Application.Interfaces.Repositories.Mqtt;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure;
using CollectManagement.Infrastructure.Persistence.Configurations.MqttConfigurations;
using CollectManagement.Infrastructure.Services;
using CollectManagement.WebAPI;
using Hangfire.Server;
using Microsoft.Extensions.FileProviders;
using MQTTnet;
using Serilog;
using Worker = CollectManagement.WebAPI.Worker;
using CollectManagement.Infrastructure.Persistence.Seed;
var builder = WebApplication.CreateBuilder(args);

#region Logging

builder.Logging.AddRinLogger();
builder.Services.AddRin();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

#endregion

#region Services

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddPresentation();

#endregion

#region MQTT

builder.Services.Configure<MqttConfig>(
    builder.Configuration.GetSection("Mqtt"));

builder.Services.AddSingleton<IMqttClient>(_ =>
{
    var factory = new MqttClientFactory();
    return factory.CreateMqttClient();
});

builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddScoped<IMqttMessageHandler, MqttMessageHandler>();
builder.Services.AddHostedService<Worker>();

#endregion

#region SignalR

builder.Services.AddSignalR();

#endregion

#region CORS (FIXED)

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins!)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

#endregion

var app = builder.Build();

#region Exception Handling

app.UseExceptionHandler(_ => { });

#endregion

#region Dev Tools

if (app.Environment.IsDevelopment())
{
    app.UseRin();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseRinDiagnosticsHandler();
}

#endregion

#region Middleware Pipeline (ORDER IS IMPORTANT)

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Endpoints

app.MapCarter();

app.MapHub<SignalRHub>("/cm/signalHub");

#endregion
await DatabaseSeeder.SeedAsync(app.Services);


Console.WriteLine("=== AVANT APP.RUN ===");
Console.WriteLine($"URLS: {string.Join(",", app.Urls)}");

app.Run();