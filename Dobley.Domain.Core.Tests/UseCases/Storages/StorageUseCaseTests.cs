using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Tests.Builders;
using Dobley.Domain.Core.Tests.UseCases.Fakes;
using Dobley.Domain.Core.UseCases.Storages;

namespace Dobley.Domain.Core.Tests.UseCases.Storages;

public class StorageUseCaseTests
{
    [Fact]
    public async Task CreateStorageCommand_AddsStorageForCurrentUserAndSavesChanges()
    {
        var storageRepository = new FakeStorageRepository();
        var commonRepository = new FakeCommonRepository();
        var handler = new CreateStorageCommandHandler(storageRepository, commonRepository);

        var storage = await handler.Handle(new CreateStorageCommand(CreateStorageForm("Fridge"), "demo"),
            CancellationToken.None);

        Assert.NotNull(storage);
        Assert.Equal("demo", storage.UserName);
        Assert.Single(storageRepository.AddedStorages);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateStorageCommand_WithOwnedStorage_UpdatesStorageAndSavesChanges()
    {
        var storage = StorageBuilder.Build(id: 1, name: "Old", userName: "demo");
        var storageRepository = new FakeStorageRepository(storage);
        var commonRepository = new FakeCommonRepository();
        var handler = new UpdateStorageCommandHandler(storageRepository, commonRepository);

        var updatedStorage = await handler.Handle(new UpdateStorageCommand(storage.Id,
            CreateStorageForm("New"), "demo"), CancellationToken.None);

        Assert.NotNull(updatedStorage);
        Assert.Equal("New", updatedStorage.Name);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteStorageCommand_WithOwnedStorage_DeletesStorageAndProducts()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var product = ProductBuilder.Build(id: 1, storageId: storage.Id);
        product.DomainStorage = storage;
        var storageRepository = new FakeStorageRepository(storage);
        var productRepository = new FakeProductRepository(product);
        var commonRepository = new FakeCommonRepository();
        var handler = new DeleteStorageCommandHandler(storageRepository, productRepository, commonRepository);

        var result = await handler.Handle(new DeleteStorageCommand(storage.Id, "demo"), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(storage, storageRepository.DeletedStorages.Single());
        Assert.Equal(product, productRepository.DeletedProducts.Single());
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task GetStoragesQuery_ReturnsOnlyCurrentUserStorages()
    {
        var firstStorage = StorageBuilder.Build(id: 1, userName: "demo");
        var secondStorage = StorageBuilder.Build(id: 2, userName: "other");
        var handler = new GetStoragesQueryHandler(new FakeStorageRepository(firstStorage, secondStorage));

        var result = await handler.Handle(new GetStoragesQuery("demo", null, null), CancellationToken.None);

        Assert.Equal(firstStorage, result.Collection.Single());
        Assert.Equal(1, result.TotalCount);
    }

    private static StorageForm CreateStorageForm(string name)
        => new()
        {
            Name = name,
            Description = "Kitchen fridge"
        };
}
