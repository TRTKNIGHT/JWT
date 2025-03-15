using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityIntegration.EF;

public class AppDbContextFactory : 
    IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(
                configuration.GetConnectionString("IdentityConnectionString"),
                b => b.MigrationsAssembly(typeof(Program).Assembly)
            );

        return new AppDbContext(builder.Options);
    }
}
