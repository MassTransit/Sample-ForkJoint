namespace ForkJoint.Api;

using MassTransit;


public class CustomEndpointNameFormatter :
    IEndpointNameFormatter
{
    readonly IEndpointNameFormatter _formatter;

    public CustomEndpointNameFormatter()
    {
        _formatter = KebabCaseEndpointNameFormatter.Instance;
    }

    public string TemporaryEndpoint(string tag)
    {
        return _formatter.TemporaryEndpoint(tag);
    }

    public string Consumer<T>()
        where T : class, IConsumer
    {
        return _formatter.Consumer<T>();
    }

    public string Message<T>()
        where T : class
    {
        return _formatter.Message<T>();
    }

    public string Saga<T>()
        where T : class, ISaga
    {
        return _formatter.Saga<T>();
    }

    public string ExecuteActivity<T, TArguments>()
        where T : class, IExecuteActivity<TArguments>
        where TArguments : class
    {
        var executeActivity = _formatter.ExecuteActivity<T, TArguments>();

        return SanitizeName(executeActivity);
    }

    public string CompensateActivity<T, TLog>()
        where T : class, ICompensateActivity<TLog>
        where TLog : class
    {
        var compensateActivity = _formatter.CompensateActivity<T, TLog>();

        return SanitizeName(compensateActivity);
    }

    public string SanitizeName(string name)
    {
        return _formatter.SanitizeName(name);
    }

    public string Separator => _formatter.Separator;
}