namespace ForkJoint.Api.Components
{
    using System.Collections.Generic;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;


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
                yield return new OrderStateMap();
                yield return new BurgerStateMap();
                yield return new OnionRingsStateMap();
                yield return new FryStateMap();
                yield return new ShakeStateMap();
                yield return new FryShakeStateMap();
                yield return new RequestStateMap();
            }
        }
    }
}