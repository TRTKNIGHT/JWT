using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityIntegration.EF.Extensions;

public static class DI
{
    public static void ConfigureIdentitySqlContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSqlServer<AppDbContext>(
            configuration.GetConnectionString("IdentityConnectionString")
        );

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
            .AddEntityFrameworkStores<AppDbContext>();
    }
}
