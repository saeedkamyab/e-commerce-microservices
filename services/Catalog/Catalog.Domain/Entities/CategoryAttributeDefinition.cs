using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public sealed class CategoryAttributeDefinition
{
    private readonly List<AttributeOption> _options = new();


    public Guid Id {  get;private set; }
    public string Name { get; private set; } = null!;
    public AttributeType Type { get; private set; }

    public bool IsRequired { get; private set; }

    public IReadOnlyCollection<AttributeOption> Options =>
    _options.AsReadOnly();

    private CategoryAttributeDefinition()
    {
        // For EF Core
    }
    private CategoryAttributeDefinition(
       Guid id,
       string name,
       AttributeType type,
       bool isRequired)
    {
        Id = id;
        Name = name;
        Type = type;
        IsRequired = isRequired;
    }

    public static CategoryAttributeDefinition Rehydrate(
    Guid id,
    string name,
    AttributeType type,
    bool isRequired)
    {
        return new CategoryAttributeDefinition(
            id,
            name,
            type,
            isRequired);
    }

    public static CategoryAttributeDefinition Create(
      string name,
      AttributeType type,
      bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Attribute name cannot be empty.",
                nameof(name));

        name = name.Trim();

        if (name.Length > 100)
            throw new ArgumentException(
                "Attribute name cannot exceed 100 characters.",
                nameof(name));

        return new CategoryAttributeDefinition(
            Guid.NewGuid(),
            name,
            type,
            isRequired);
    }

    public void AddOption(AttributeOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        if (Type != AttributeType.Option)
            throw new InvalidOperationException(
                "Options can only be added to Option attributes.");

        if (_options.Contains(option))
            return;

        _options.Add(option);
    }

    public void RemoveOption(AttributeOption option)
    {
        ArgumentNullException.ThrowIfNull(option);

        if (Type != AttributeType.Option)
            throw new InvalidOperationException(
                "Options can only be removed from Option attributes.");

        _options.Remove(option);
    }
}
