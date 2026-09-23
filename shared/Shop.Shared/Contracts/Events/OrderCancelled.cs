namespace Shop.Shared.Contracts.Events;

public record OrderCancelled(
    Guid OrderId,
    string Reason,
    DateTimeOffset OccurredAt,
    IReadOnlyList<OrderPlacedItem> Items
);