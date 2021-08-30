using ForkJoint.Application.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Reflection;

namespace ForkJoint.Application
{
    public class ForkJointSagaDbContextFactory : IDesignTimeDbContextFactory<ForkJointSagaDbContext>
    {
        public ForkJointSagaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ForkJointSagaDbContext>();

            string connectionString = "Server=tcp:localhost,1434;Database=ForkJoint;Persist Security Info=False;User ID=sa;Password=Password12!;Encrypt=False;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString, m =>
            {
                m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                m.MigrationsHistoryTable($"__{nameof(ForkJointSagaDbContext)}");
            });

            return new ForkJointSagaDbContext(optionsBuilder.Options);
        }
    }
}
