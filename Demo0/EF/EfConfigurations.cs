using Demo0.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Demo0.EF;

public partial class AppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.Property(ur => ur.RoleId)
                .IsRequired();
            entity.Property(ur => ur.UserId)
                .IsRequired();

            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId);
            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(u => u.Email)
                .IsRequired();
            entity.Property(u => u.Username)
                .IsRequired();
            entity.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email)
                  .HasDatabaseName("IX_Unique_Email")
                  .IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(r => r.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasKey(r => r.Id);

            entity.HasData(
                new Role { Id = 1, Name = "Admin", Description = "Admin Role" },
                new Role { Id = 2, Name = "Editor", Description = " Editor Role" },
                new Role { Id = 3, Name = "User", Description = "User Role" }
            );
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(c => c.ClientId)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(c => c.ClientUrl)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasKey(c => c.Id);

            entity.HasData(
                new Client
                {
                    Id = 1,
                    ClientId = "Client1",
                    Name = "Client Application 1",
                    ClientUrl = "https://client1.com"
                },
                new Client
                {
                    Id = 2,
                    ClientId = "Client2",
                    Name = "Client Application 2",
                    ClientUrl = "https://client2.com"
                }
            );
        });

        modelBuilder.Entity<SigningKey>(entity =>
        {
            entity.Property(sk => sk.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(sk => sk.KeyId)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(sk => sk.PrivateKey)
                .IsRequired();
            entity.Property(sk => sk.PublicKey)
                .IsRequired();
            entity.Property(sk => sk.IsActive)
                .IsRequired();
            entity.Property(sk => sk.CreatedAt)
                .IsRequired();
            entity.Property(sk => sk.ExpiresAt)
                .IsRequired();

            entity.HasKey(sk => sk.Id);
            entity.HasIndex(sk => sk.KeyId);
        });
    }
}
