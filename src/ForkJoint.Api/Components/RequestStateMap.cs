namespace ForkJoint.Api.Components
{
    using Automatonymous.Requests;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;


    public class RequestStateMap :
        SagaClassMap<RequestState>
    {
        protected override void Configure(EntityTypeBuilder<RequestState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.ConversationId);
            entity.Property(x => x.ResponseAddress);
            entity.Property(x => x.FaultAddress);
            entity.Property(x => x.ExpirationTime);

            entity.Property(x => x.SagaCorrelationId);
            entity.Property(x => x.SagaAddress);

            entity.HasIndex(x => x.SagaCorrelationId).IsUnique();
        }
    }
}