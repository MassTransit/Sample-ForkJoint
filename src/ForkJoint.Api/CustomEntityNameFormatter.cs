namespace ForkJoint.Api
{
    using System;
    using System.Linq;
    using MassTransit;
    using MassTransit.Futures.Contracts;
    using MassTransit.Internals;


    public class CustomEntityNameFormatter :
        IEntityNameFormatter
    {
        readonly IEntityNameFormatter _entityNameFormatter;

        public CustomEntityNameFormatter(IEntityNameFormatter entityNameFormatter)
        {
            _entityNameFormatter = entityNameFormatter;
        }

        public string FormatEntityName<T>()
        {
            if (typeof(T).ClosesType(typeof(Get<>), out Type[] types))
            {
                var name = (string)typeof(IEntityNameFormatter)
                    .GetMethod("FormatEntityName")
                    .MakeGenericMethod(types)
                    .Invoke(_entityNameFormatter, Array.Empty<object>());

                var suffix = typeof(T).Name.Split('`').First();

                return $"{name}-{suffix}";
            }

            return _entityNameFormatter.FormatEntityName<T>();
        }
    }
}