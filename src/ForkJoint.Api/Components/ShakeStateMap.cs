namespace ForkJoint.Api.Components
{
    using MassTransit;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StateMachines;


    public class ShakeStateMap :
        SagaClassMap<ShakeState>
    {
        protected override void Configure(EntityTypeBuilder<ShakeState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.Created);
            entity.Property(x => x.Completed);
            entity.Property(x => x.Faulted);

            entity.Property(x => x.ExceptionInfo).HasConversion(new JsonValueConverter<ExceptionInfo>())
                .Metadata.SetValueComparer(new JsonValueComparer<ExceptionInfo>());

            entity.Property(x => x.Flavor);
            entity.Property(x => x.Size);

            entity.Property(x => x.RequestId);
            entity.Property(x => x.ResponseAddress);
        }
    }
}