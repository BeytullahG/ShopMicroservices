using System.Collections.ObjectModel;

namespace Shop.Shared.Contracts.Events;
public record OrderPlaced(
    Guid OrderId,
    Guid UserId,
    IReadOnlyList<OrderPlacedItem> Items,
    DateTimeOffset OccurredAt
);

