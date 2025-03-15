using Demo0.EF;
using Demo0.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Demo0.Services;

public class KeyRotationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(10);

    public KeyRotationService(IServiceProvider serviceProvider)
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
        await using var context = scope
            .ServiceProvider.GetRequiredService<AppDbContext>();
        var activeKey = await context.SigningKeys
            .FirstOrDefaultAsync(k => k.IsActive);

        if (
            activeKey is null ||
            activeKey.ExpiresAt <= DateTime.Now.AddDays(15)
        )
        {
            using var transaction = await context
                .Database.BeginTransactionAsync();
            try
            {
                if (activeKey is not null)
                {
                    activeKey.IsActive = false;
                    context.SigningKeys.Update(activeKey);
                }

                using var rsa = RSA.Create(2048);
                var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                var newKeyId = Guid.NewGuid().ToString();

                var newKey = new SigningKey
                {
                    KeyId = newKeyId,
                    PrivateKey = privateKey,
                    PublicKey = publicKey,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMonths(6)
                };

                await context.SigningKeys.AddAsync(newKey);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
            }
        }
    }
}
