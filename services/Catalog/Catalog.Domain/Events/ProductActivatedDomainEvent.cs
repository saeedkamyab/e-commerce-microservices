using SharedKernel.Domain;

namespace Catalog.Domain.Events;

public sealed record ProductActivatedDomainEvent(
    Guid ProductId,
    DateTime OccurredOnUtc
) : IDomainEvent;
