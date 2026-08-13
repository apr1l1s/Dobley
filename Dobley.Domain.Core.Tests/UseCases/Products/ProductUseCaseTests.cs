using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Forms;
using Dobley.Domain.Core.Tests.Builders;
using Dobley.Domain.Core.Tests.UseCases.Fakes;
using Dobley.Domain.Core.UseCases.Products;

namespace Dobley.Domain.Core.Tests.UseCases.Products;

public class ProductUseCaseTests
{
    [Fact]
    public async Task CreateProductCommand_WithOwnedStorage_AddsProductAndSavesChanges()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var productRepository = new FakeProductRepository();
        var storageRepository = new FakeStorageRepository(storage);
        var commonRepository = new FakeCommonRepository();
        var handler = new CreateProductCommandHandler(productRepository, storageRepository, commonRepository);

        var product = await handler.Handle(new CreateProductCommand(CreateProductForm(storage.Id), "demo"),
            CancellationToken.None);

        Assert.NotNull(product);
        Assert.Equal(storage.Id, product.StorageId);
        Assert.Single(productRepository.AddedProducts);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateProductCommand_WithForeignStorage_ReturnsNullWithoutSaving()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "owner");
        var productRepository = new FakeProductRepository();
        var storageRepository = new FakeStorageRepository(storage);
        var commonRepository = new FakeCommonRepository();
        var handler = new CreateProductCommandHandler(productRepository, storageRepository, commonRepository);

        var product = await handler.Handle(new CreateProductCommand(CreateProductForm(storage.Id), "demo"),
            CancellationToken.None);

        Assert.Null(product);
        Assert.Empty(productRepository.AddedProducts);
        Assert.Equal(0, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateProductCommand_WithOwnedProduct_UpdatesProductAndSavesChanges()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var product = ProductBuilder.Build(id: 1, name: "Milk", storageId: storage.Id);
        product.DomainStorage = storage;
        var productRepository = new FakeProductRepository(product);
        var commonRepository = new FakeCommonRepository();
        var handler = new UpdateProductCommandHandler(productRepository, commonRepository);

        var updatedProduct = await handler.Handle(new UpdateProductCommand(product.Id,
            CreateProductForm(storage.Id, name: "Bread"), "demo"), CancellationToken.None);

        Assert.NotNull(updatedProduct);
        Assert.Equal("Bread", updatedProduct.Name);
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteProductCommand_WithOwnedProduct_DeletesProductAndSavesChanges()
    {
        var storage = StorageBuilder.Build(id: 1, userName: "demo");
        var product = ProductBuilder.Build(id: 1, storageId: storage.Id);
        product.DomainStorage = storage;
        var productRepository = new FakeProductRepository(product);
        var commonRepository = new FakeCommonRepository();
        var handler = new DeleteProductCommandHandler(productRepository, commonRepository);

        var result = await handler.Handle(new DeleteProductCommand(product.Id, "demo"), CancellationToken.None);

        Assert.True(result);
        Assert.Equal(product, productRepository.DeletedProducts.Single());
        Assert.Equal(1, commonRepository.SaveChangesCount);
    }

    [Fact]
    public async Task GetProductsQuery_ReturnsOnlyCurrentUserProducts()
    {
        var firstStorage = StorageBuilder.Build(id: 1, userName: "demo");
        var secondStorage = StorageBuilder.Build(id: 2, userName: "other");
        var firstProduct = ProductBuilder.Build(id: 1, storageId: firstStorage.Id);
        var secondProduct = ProductBuilder.Build(id: 2, storageId: secondStorage.Id);
        firstProduct.DomainStorage = firstStorage;
        secondProduct.DomainStorage = secondStorage;
        var handler = new GetProductsQueryHandler(new FakeProductRepository(firstProduct, secondProduct));

        var result = await handler.Handle(new GetProductsQuery("demo", null, null), CancellationToken.None);

        Assert.Equal(firstProduct, result.Collection.Single());
        Assert.Equal(1, result.TotalCount);
    }

    private static ProductForm CreateProductForm(int storageId, string name = "Milk")
        => new()
        {
            Name = name,
            Description = "Fresh milk",
            Category = Category.Dairy,
            Unit = 1,
            UnitType = UnitType.Liters,
            Price = 120,
            Barcode = "4600000000000",
            StorageId = storageId,
            ExpirationDate = DateTime.UtcNow.AddDays(3)
        };
}
