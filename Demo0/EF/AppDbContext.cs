using Demo0.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Demo0.EF;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<SigningKey> SigningKeys { get; set; }
}
