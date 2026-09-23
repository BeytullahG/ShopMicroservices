namespace Shop.Shared.Contracts.Events;

public record OrderPaid(
    Guid OrderId,
    DateTimeOffset PaidAt
);