using Carter;
using CollectManagement.Application;
using CollectManagement.Application.Handlers;
using CollectManagement.Application.Interfaces.Repositories.Mqtt;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Infrastructure;
using CollectManagement.Infrastructure.Persistence.Configurations.MqttConfigurations;
using CollectManagement.Infrastructure.Services;
using CollectManagement.Infrastructure.Persistence.Seed;
using MQTTnet;
using Serilog;
using Worker = CollectManagement.WebAPI.Worker;
using CollectManagement.WebAPI;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);


#region Logging

builder.Logging.AddConsole();
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


#region CORS

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

#endregion



var app = builder.Build();



#region TEST LOGS

Console.WriteLine("===============================");
Console.WriteLine(" APPLICATION STARTING ");
Console.WriteLine("===============================");

Console.WriteLine(
    $"Environment : {app.Environment.EnvironmentName}"
);


Console.WriteLine(
    $"Application URLs : {string.Join(",", app.Urls)}"
);


Console.WriteLine(
    "Swagger middleware activated"
);


#endregion



#region Exception

app.UseExceptionHandler("/error");

#endregion



#region Swagger

app.UseSwagger();


app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "v1/swagger.json",
        "Gestion Alarme API V1"
    );

    options.RoutePrefix = "swagger";
});


#endregion



#region Middleware

// Désactiver temporairement pour AKS
// sinon il cherche un HTTPS qui n'existe pas
// app.UseHttpsRedirection();


app.UseSerilogRequestLogging();


app.UseRouting();


app.UseCors("CorsPolicy");


app.UseAuthentication();


app.UseAuthorization();

#endregion



#region Endpoints


app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "API Running",
        environment = app.Environment.EnvironmentName,
        swagger = "/swagger"
    });
});


app.MapCarter();


app.MapHub<SignalRHub>(
    "/cm/signalHub"
);
app.UseHttpMetrics();

app.MapMetrics();

#endregion



await DatabaseSeeder.SeedAsync(app.Services);



Console.WriteLine("===============================");
Console.WriteLine(" API READY ");
Console.WriteLine("===============================");



app.Run();