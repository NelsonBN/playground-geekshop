namespace GeekShop.Api.Catalogs;

public sealed class Catalog
{
    public const int NAME_MAX_LENGTH = 200;
    public const int DESCRIPTION_MAX_LENGTH = 500;

    // Required by EF Core to materialize the entity; the domain creates catalogs with Create.
    private Catalog() { }

    public Guid Id { get; private init; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public static Catalog Create(string name, string? description)
    {
        Validate(name, description);

        return new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description)
    {
        Validate(name, description);

        Name = name;
        Description = description;
    }

    private static void Validate(string name, string? description)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidCatalogException("The catalog name is required.");
        }

        if(name.Length > NAME_MAX_LENGTH)
        {
            throw new InvalidCatalogException($"The catalog name cannot exceed {NAME_MAX_LENGTH} characters.");
        }

        if(description?.Length > DESCRIPTION_MAX_LENGTH)
        {
            throw new InvalidCatalogException(
                $"The catalog description cannot exceed {DESCRIPTION_MAX_LENGTH} characters.");
        }
    }
}
