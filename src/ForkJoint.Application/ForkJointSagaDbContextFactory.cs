using ForkJoint.Application.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ForkJoint.Application
{
    public class ForkJointSagaDbContextFactory : IDesignTimeDbContextFactory<ForkJointSagaDbContext>
    {
        public ForkJointSagaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ForkJointSagaDbContext>();

            optionsBuilder.UseSqlServer("Server=tcp:localhost,1434;Database=ForkJoint;Persist Security Info=False;User ID=sa;Password=Password12!;Encrypt=False;TrustServerCertificate=True;");

            return new ForkJointSagaDbContext(optionsBuilder.Options);
        }
    }
}
