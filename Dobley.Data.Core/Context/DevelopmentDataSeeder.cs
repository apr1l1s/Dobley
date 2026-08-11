using Dobley.Data.Core.Repositories.Users;
using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dobley.Data.Core.Context;

public static class DevelopmentDataSeeder
{
    private const string SEED_DEV_DATA_VARIABLE = "SEED_DEV_DATA";
    private const string DEMO_LOGIN = "demo";
    private const string DEMO_PASSWORD = "password";
    private const string DEMO_STORAGE_NAME = "Домашний холодильник";
    private const string DEMO_PRODUCT_BARCODE = "4600000000000";

    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        if (!bool.TryParse(Environment.GetEnvironmentVariable(SEED_DEV_DATA_VARIABLE), out var shouldSeed) ||
            !shouldSeed)
        {
            return;
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DobleyContext>();
        var commonRepository = scope.ServiceProvider.GetRequiredService<ICommonRepository>();

        var user = await context.Users.FirstOrDefaultAsync(x => x.Login == DEMO_LOGIN, cancellationToken);
        if (user == null)
        {
            user = User.Create(DEMO_LOGIN, AuthService.HashPassword(DEMO_PASSWORD));
            await context.Users.AddAsync(user, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);
        }

        var storage = await context.Storages
            .FirstOrDefaultAsync(x => x.UserName == DEMO_LOGIN && x.Name == DEMO_STORAGE_NAME, cancellationToken);

        if (storage == null)
        {
            storage = Storage.Create(DEMO_STORAGE_NAME, "Тестовое хранилище для локальной разработки", user);
            await context.Storages.AddAsync(storage, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);
        }

        var hasProduct = await context.Products.AnyAsync(x => x.StorageId == storage.Id &&
                                                              x.Barcode == DEMO_PRODUCT_BARCODE, cancellationToken);
        if (hasProduct)
        {
            return;
        }

        var product = Product.Create("Молоко", "Тестовый продукт для локальной разработки", Category.Dairy, 1,
            UnitType.Liters, 89.90m, DEMO_PRODUCT_BARCODE, storage, DateTime.UtcNow.AddDays(2));
        await context.Products.AddAsync(product, cancellationToken);
        await commonRepository.SaveChangesAsync(cancellationToken);
    }
}
