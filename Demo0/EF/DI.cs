namespace Demo0.EF;

public static class DI
{
    public static void ConfigureSqlServerContext(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSqlServer<AppDbContext>(
            configuration.GetConnectionString("Connection")
        );
    }
}
