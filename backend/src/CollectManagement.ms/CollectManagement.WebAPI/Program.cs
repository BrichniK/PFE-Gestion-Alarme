using Carter;
using CollectManagement.Infrastructure.Services;
using CollectManagement.WebAPI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Logging

builder.Logging.AddRinLogger();
builder.Services.AddRin();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

#endregion

#region Services

builder.Services
    

    .AddPresentation();

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

//app.MapHub<SignalRHub>("/cm/signalHub");

#endregion

app.Run();