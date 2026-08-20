namespace Catalog.Infrastructure.Persistence.Models;

internal sealed class AttributeOptionRecord
{
    public Guid Id { get; set; }

    public Guid AttributeDefinitionId { get; set; }

    public string Value { get; set; } = null!;
}
