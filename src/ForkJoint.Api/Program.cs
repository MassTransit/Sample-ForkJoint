namespace ForkJoint.Api;

using System;
using Destructurama;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Exceptions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Enrichers.Span;
using Serilog.Sinks.Grafana.Loki;

public class Program
{ 
    static LoggingLevelSwitch _levelSwitch = new() { MinimumLevel = LogEventLevel.Verbose };
    
    static bool? _isRunningInContainer;
    
    static bool IsRunningInContainer =>
        _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;
    
    public static int Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .ConfigureDefaults(_levelSwitch)
            .WriteTo.Seq(IsRunningInContainer ? "http://seq:5341" : "http://localhost:5341", controlLevelSwitch: _levelSwitch)
            .WriteTo.GrafanaLoki(IsRunningInContainer ? "http://loki:3100" : "http://localhost:3100")
            .CreateBootstrapLogger();
        
        try
        {
            Log.Information("Starting web host");
            CreateHostBuilder(args).Build().Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ConfigureDefaults(_levelSwitch)
                    .WriteTo.Console()
                    .WriteTo.Seq(IsRunningInContainer ? "http://seq:5341" : "http://localhost:5341", controlLevelSwitch: _levelSwitch)
                    .WriteTo.GrafanaLoki(IsRunningInContainer ? "http://loki:3100" : "http://localhost:3100")
                    .WriteTo.ApplicationInsights(services.GetRequiredService<TelemetryConfiguration>(), TelemetryConverter.Traces);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseStartup<Startup>();
            });
    }
}