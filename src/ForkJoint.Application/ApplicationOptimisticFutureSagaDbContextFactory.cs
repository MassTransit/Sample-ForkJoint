using ForkJoint.Application.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace ForkJoint.Application
{
    public class ApplicationOptimisticFutureSagaDbContextFactory : IDesignTimeDbContextFactory<ApplicationOptimisticFutureSagaDbContext>
    {
        public ApplicationOptimisticFutureSagaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationOptimisticFutureSagaDbContext>();

            string connectionString = "Server=tcp:localhost,1434;Database=ForkJoint;Persist Security Info=False;User ID=sa;Password=Password12!;Encrypt=False;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString, m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                m.MigrationsHistoryTable($"__{nameof(ApplicationOptimisticFutureSagaDbContext)}");
            });

            return new ApplicationOptimisticFutureSagaDbContext(optionsBuilder.Options);
        }
    }
}
