namespace Shop.Shared.Contracts.Events;

public record OrderLine(
    Guid ProductId,
    int Quantity
);