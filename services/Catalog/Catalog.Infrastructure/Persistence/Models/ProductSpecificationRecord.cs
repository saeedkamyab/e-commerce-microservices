namespace Catalog.Infrastructure.Persistence.Models;


internal sealed class ProductSpecificationRecord
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid AttributeDefinitionId { get; set; }

    public string? TextValue { get; set; }

    public int? NumberValue { get; set; }

    public decimal? DecimalValue { get; set; }

    public bool? BooleanValue { get; set; }

    public DateTime? DateValue { get; set; }

    public string? OptionValue { get; set; }
}
