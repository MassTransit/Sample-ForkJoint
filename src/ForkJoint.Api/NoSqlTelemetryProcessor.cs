namespace ForkJoint.Api;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;


public class NoSqlTelemetryProcessor :
    ITelemetryProcessor
{
    public NoSqlTelemetryProcessor(ITelemetryProcessor next)
    {
        Next = next;
    }

    ITelemetryProcessor Next { get; set; }

    public void Process(ITelemetry item)
    {
        if (IsSqlDependency(item))
            return;

        Next.Process(item);
    }

    static bool IsSqlDependency(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;

        return dependency?.Type == "SQL";
    }
}