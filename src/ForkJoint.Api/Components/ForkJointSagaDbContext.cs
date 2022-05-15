namespace ForkJoint.Api.Components;

using System.Collections.Generic;
using MassTransit.EntityFrameworkCoreIntegration;
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
        get { yield return new FutureStateMap(); }
    }
}