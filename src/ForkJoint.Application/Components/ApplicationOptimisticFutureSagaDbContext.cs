namespace ForkJoint.Application.Components
{
    using MassTransit.EntityFrameworkCoreIntegration;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationOptimisticFutureSagaDbContext :
        OptimisticFutureSagaDbContext
    {
        public ApplicationOptimisticFutureSagaDbContext(DbContextOptions options)
            : base(options)
        {
        }
    }
}