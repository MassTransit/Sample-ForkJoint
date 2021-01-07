namespace ForkJoint.Api.Components
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StateMachines;


    public class OrderStateMap :
        SagaClassMap<OrderState>
    {
        protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.Created);
            entity.Property(x => x.Updated);
            entity.Property(x => x.Completed);
            entity.Property(x => x.Faulted);

            entity.Property(x => x.LineCount);

            entity.Property(x => x.RequestId);
            entity.Property(x => x.ResponseAddress);

            entity.Property(x => x.LinesPending)
                .HasConversion(new JsonValueConverter<HashSet<Guid>>())
                .Metadata.SetValueComparer(new JsonValueComparer<HashSet<Guid>>());
            entity.Property(x => x.LinesCompleted)
                .HasConversion(new JsonValueConverter<Dictionary<Guid, OrderLineCompleted>>())
                .Metadata.SetValueComparer(new JsonValueComparer<Dictionary<Guid, OrderLineCompleted>>());
            entity.Property(x => x.LinesFaulted)
                .HasConversion(new JsonValueConverter<Dictionary<Guid, OrderLineFaulted>>())
                .Metadata.SetValueComparer(new JsonValueComparer<Dictionary<Guid, OrderLineFaulted>>());
        }
    }
}