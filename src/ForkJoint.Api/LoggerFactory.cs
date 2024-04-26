namespace ForkJoint.Api;

using Destructurama;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;


public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration loggerConfiguration, LoggingLevelSwitch levelSwitch)
    {
        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.ControlledBy(levelSwitch)
            .Destructure.UsingAttributes()
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithExceptionDetails();

        return loggerConfiguration;
    }
}