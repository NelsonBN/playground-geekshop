using GeekShop.Api.IntegrationTests.Fixtures;
using GeekShop.Api.Products;
using Microsoft.Extensions.DependencyInjection;

namespace GeekShop.Api.IntegrationTests.Products;

[Collection(nameof(DatabaseCollection))]
[Trait("Category", "Integration")]
public sealed class ProductsRepositoryTests(DatabaseFixture database) : IAsyncDisposable
{
    private readonly ApiFactory _factory = new(database.ConnectionString);

    public async ValueTask DisposeAsync()
        => await _factory.DisposeAsync();

    private static Product _newProduct()
        => new() { Id = Guid.NewGuid(), Name = $"Keyboard-{Guid.NewGuid()}", Quantity = 3, Price = 9.5 };

    private static IProductsRepository _repositoryOf(AsyncServiceScope scope)
        => scope.ServiceProvider.GetRequiredService<IProductsRepository>();

    private async Task<Product> _seedAsync()
    {
        var product = _newProduct();
        await using var scope = _factory.Services.CreateAsyncScope();
        await _repositoryOf(scope).AddAsync(product, TestContext.Current.CancellationToken);

        return product;
    }

    [Fact]
    public async Task Given_AddedProduct_When_Get_Then_ReturnsPersistedProduct()
    {
        // Arrange
        var product = await _seedAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var result = await repository.GetAsync(product.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task Given_UnknownId_When_Get_Then_ReturnsNull()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var result = await repository.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_AddedProduct_When_List_Then_ContainsIt()
    {
        // Arrange
        var product = await _seedAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var result = await repository.ListAsync(TestContext.Current.CancellationToken);

        // Assert
        result.Should().ContainEquivalentOf(product);
    }

    [Fact]
    public async Task Given_ExistingProduct_When_Update_Then_ChangesArePersisted()
    {
        // Arrange
        var product = await _seedAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);
        var changed = new Product { Id = product.Id, Name = $"Changed-{product.Id}", Quantity = 9, Price = 1.25 };

        // Act
        await repository.UpdateAsync(changed, TestContext.Current.CancellationToken);

        // Assert
        await using var verification = _factory.Services.CreateAsyncScope();
        var stored = await _repositoryOf(verification).GetAsync(
            product.Id, TestContext.Current.CancellationToken);
        stored.Should().BeEquivalentTo(changed);
    }

    [Fact]
    public async Task Given_ExistingProduct_When_Delete_Then_ProductIsRemoved()
    {
        // Arrange
        var product = await _seedAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        await repository.DeleteAsync(product.Id, TestContext.Current.CancellationToken);

        // Assert
        var exists = await repository.ExistsAsync(product.Id, TestContext.Current.CancellationToken);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Given_UnknownId_When_Delete_Then_DoesNotThrow()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var act = () => repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Given_ExistingProduct_When_Exists_Then_ReturnsTrue()
    {
        // Arrange
        var product = await _seedAsync();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var result = await repository.ExistsAsync(product.Id, TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Given_UnknownId_When_Exists_Then_ReturnsFalse()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = _repositoryOf(scope);

        // Act
        var result = await repository.ExistsAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        // Assert
        result.Should().BeFalse();
    }
}
