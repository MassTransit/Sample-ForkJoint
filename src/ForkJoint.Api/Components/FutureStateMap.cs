namespace ForkJoint.Api.Components
{
    using System;
    using System.Collections.Generic;
    using ForkJoint.Components;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class FutureStateMap :
        SagaClassMap<FutureState>
    {
        protected override void Configure(EntityTypeBuilder<FutureState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.Created);
            entity.Property(x => x.Deadline);
            entity.Property(x => x.Completed);
            entity.Property(x => x.Canceled);
            entity.Property(x => x.Faulted);

            entity.Property(x => x.Location);

            entity.Property(x => x.RetryAttempt);

            entity.Property(x => x.Request).HasConversion(new JsonValueConverter<FutureMessage>())
                .Metadata.SetValueComparer(new JsonValueComparer<FutureMessage>());
            entity.Property(x => x.Pending)
                .HasConversion(new JsonValueConverter<HashSet<Guid>>())
                .Metadata.SetValueComparer(new JsonValueComparer<HashSet<Guid>>());
            entity.Property(x => x.Results)
                .HasConversion(new JsonValueConverter<Dictionary<Guid, FutureMessage>>())
                .Metadata.SetValueComparer(new JsonValueComparer<Dictionary<Guid, FutureMessage>>());
            entity.Property(x => x.Faults)
                .HasConversion(new JsonValueConverter<Dictionary<Guid, FutureMessage>>())
                .Metadata.SetValueComparer(new JsonValueComparer<Dictionary<Guid, FutureMessage>>());

            entity.Property(x => x.Subscriptions)
                .HasConversion(new JsonValueConverter<HashSet<FutureSubscription>>())
                .Metadata.SetValueComparer(new JsonValueComparer<HashSet<FutureSubscription>>());
            entity.Property(x => x.Variables)
                .HasConversion(new JsonValueConverter<Dictionary<string, object>>())
                .Metadata.SetValueComparer(new JsonValueComparer<Dictionary<string, object>>());
        }
    }
}