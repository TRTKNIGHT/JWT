using JWTAuthServer.Data;
using JWTAuthServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace JWTAuthServer.Services;

public class KeyRotationService : BackgroundService
{
    private static IServiceProvider _serviceProvider;
    private static TimeSpan _rotationInterval = TimeSpan.FromDays(10);

    public KeyRotationService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RotateKeyAsync();
            await Task.Delay(_rotationInterval, stoppingToken);
        }
    }

    private async Task RotateKeyAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var activeKey = await context.SigningKeys.FirstOrDefaultAsync(s => s.IsActive);

        if (activeKey is null ||
            activeKey.ExpiresAt < DateTime.Now.AddDays(15))
        {
            if (activeKey is not null)
            {
                activeKey.IsActive = false;
                context.SigningKeys.Update(activeKey);
                await context.SaveChangesAsync();
            }
        }

        using var rsa = RSA.Create(2048);
        var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
        var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        var keyId = Guid.NewGuid().ToString();

        var key = new SigningKey
        {
            KeyId = keyId,
            PrivateKey = privateKey,
            PublicKey = publicKey,
            IsActive = true,
            CreatedAt = DateTime.Now,
            ExpiresAt = DateTime.Now.AddMonths(6)
        };

        await context.SigningKeys.AddAsync(key);
        await context.SaveChangesAsync();
    }
}
