namespace ForkJoint.Application.Components
{
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;

    public class ForkJointSagaDbContext :
        SagaDbContext
    {
        public ForkJointSagaDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new FutureStateMap();
                yield return new OptimisticConcurrencyTestsStateMap();
            }
        }
    }
}