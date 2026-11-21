using GeekShop.Api.Catalogs;
using GeekShop.Api.Catalogs.GetCatalogs;

namespace GeekShop.Api.Tests.Catalogs.GetCatalogs;

public sealed class GetCatalogsHandlerTests
{
    [Fact]
    public async Task Given_Catalogs_When_Handle_Then_ReturnsIdAndNameOfEach()
    {
        // Arrange
        var first = Catalog.Create("Summer", "Deals");
        var second = Catalog.Create("Winter", null);
        var repository = Substitute.For<ICatalogsRepository>();
        repository.ListAsync(CancellationToken.None).Returns([first, second]);
        var handler = new GetCatalogsHandler(repository);

        // Act
        var result = await handler.HandleAsync(CancellationToken.None);

        // Assert
        result.Should().Equal(
            new GetCatalogsResponse(first.Id, "Summer"),
            new GetCatalogsResponse(second.Id, "Winter"));
    }
}
