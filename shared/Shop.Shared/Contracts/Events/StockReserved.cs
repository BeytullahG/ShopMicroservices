namespace Shop.Shared.Contracts.Events;

public record StockReserved(
    Guid OrderId,
    DateTimeOffset OccurredAt
);