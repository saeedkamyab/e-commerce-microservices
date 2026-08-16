using SharedKernel.Domain;

namespace Catalog.Domain.Events;

public sealed record ProductPriceChangedDomainEvent(
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    DateTime OccurredOnUtc) : IDomainEvent;

