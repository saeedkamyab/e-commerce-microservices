using Catalog.Domain.Enums;

namespace Catalog.Infrastructure.Persistence.Models;

internal sealed class CategoryAttributeDefinitionRecord
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public AttributeType Type { get; set; }

    public bool IsRequired { get; set; }
}
