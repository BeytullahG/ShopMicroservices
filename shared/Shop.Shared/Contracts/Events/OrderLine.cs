namespace Shop.Shared.Contracts.Events;
public record OrderPlacedItem(
    Guid ProductId,
    int Quantity
);