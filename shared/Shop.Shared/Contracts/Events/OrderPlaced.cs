namespace Shop.Shared.Contracts.Events;

public record OrderPlaced(
    Guid OrderId,
    Guid UserId,
    IReadOnlyList<OrderLine> Items,
    DateTimeOffset OccurredAt
);

