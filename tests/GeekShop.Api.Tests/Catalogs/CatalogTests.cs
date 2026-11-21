using GeekShop.Api.Catalogs;

namespace GeekShop.Api.Tests.Catalogs;

public sealed class CatalogTests
{
    [Fact]
    public void Given_ValidData_When_Create_Then_ReturnsCatalogWithNewId()
    {
        // Arrange
        var name = "Summer";
        var description = "Summer deals";

        // Act
        var catalog = Catalog.Create(name, description);

        // Assert
        catalog.Id.Should().NotBeEmpty();
        catalog.Name.Should().Be("Summer");
        catalog.Description.Should().Be("Summer deals");
    }

    [Fact]
    public void Given_NoDescription_When_Create_Then_AcceptsIt()
    {
        // Arrange
        string? description = null;

        // Act
        var catalog = Catalog.Create("Summer", description);

        // Assert
        catalog.Description.Should().BeNull();
    }

    [Fact]
    public void Given_LimitLengths_When_Create_Then_AcceptsThem()
    {
        // Arrange
        var name = new string('n', Catalog.NAME_MAX_LENGTH);
        var description = new string('d', Catalog.DESCRIPTION_MAX_LENGTH);

        // Act
        var catalog = Catalog.Create(name, description);

        // Assert
        catalog.Name.Should().HaveLength(Catalog.NAME_MAX_LENGTH);
        catalog.Description.Should().HaveLength(Catalog.DESCRIPTION_MAX_LENGTH);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_BlankName_When_Create_Then_Throws(string name)
    {
        // Arrange
        string? description = null;

        // Act
        var act = () => Catalog.Create(name, description);

        // Assert
        act.Should().Throw<InvalidCatalogException>().WithMessage("*name*required*");
    }

    [Fact]
    public void Given_NameOverLimit_When_Create_Then_Throws()
    {
        // Arrange
        var name = new string('n', Catalog.NAME_MAX_LENGTH + 1);

        // Act
        var act = () => Catalog.Create(name, null);

        // Assert
        act.Should().Throw<InvalidCatalogException>().WithMessage("*name*200*");
    }

    [Fact]
    public void Given_DescriptionOverLimit_When_Create_Then_Throws()
    {
        // Arrange
        var description = new string('d', Catalog.DESCRIPTION_MAX_LENGTH + 1);

        // Act
        var act = () => Catalog.Create("Summer", description);

        // Assert
        act.Should().Throw<InvalidCatalogException>().WithMessage("*description*500*");
    }

    [Fact]
    public void Given_ValidData_When_Update_Then_ChangesNameAndDescriptionKeepingId()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", "Summer deals");
        var id = catalog.Id;

        // Act
        catalog.Update("Winter", null);

        // Assert
        catalog.Id.Should().Be(id);
        catalog.Name.Should().Be("Winter");
        catalog.Description.Should().BeNull();
    }

    [Fact]
    public void Given_InvalidData_When_Update_Then_ThrowsAndKeepsTheCurrentState()
    {
        // Arrange
        var catalog = Catalog.Create("Summer", "Summer deals");

        // Act
        var act = () => catalog.Update(" ", "Other");

        // Assert
        act.Should().Throw<InvalidCatalogException>();
        catalog.Name.Should().Be("Summer");
        catalog.Description.Should().Be("Summer deals");
    }
}
