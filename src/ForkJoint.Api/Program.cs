using Serilog.Sinks.Grafana.Loki;

namespace ForkJoint.Api;

using System;
using Destructurama;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Serilog.Exceptions;

public class Program
{
    static bool? _isRunningInContainer;
    
    static bool IsRunningInContainer =>
        _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;
    
    public static int Main(string[] args)
    {
        var levelSwitch = new LoggingLevelSwitch { MinimumLevel = LogEventLevel.Verbose };
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.ControlledBy(levelSwitch)
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console()
            .WriteTo.Seq(IsRunningInContainer ? "http://seq:5341" : "http://localhost:5341", controlLevelSwitch: levelSwitch)
            .WriteTo.GrafanaLoki(IsRunningInContainer ? "http://loki:3100" : "http://localhost:3100")
            .CreateLogger();
        
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
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseStartup<Startup>();
            });
    }
}