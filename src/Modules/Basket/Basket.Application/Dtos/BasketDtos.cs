namespace Basket.Application.Dtos;

public record BasketDto(
    Guid BuyerId,
    List<BasketItemDto> Items,
    decimal TotalPrice,
    DateTime LastUpdated);

public record BasketItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    string? ImageUrl,
    decimal TotalPrice);

public record AddToBasketRequest(
    Guid ProductId,
    int Quantity);

public record UpdateBasketItemRequest(int Quantity);
