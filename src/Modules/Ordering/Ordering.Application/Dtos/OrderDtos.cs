using Ordering.Domain.Enums;

namespace Ordering.Application.Dtos;

public record OrderDto(
    Guid Id,
    Guid BuyerId,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    List<OrderItemDto> Items,
    AddressDto ShippingAddress);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public record AddressDto(
    string Street,
    string City,
    string PostalCode,
    string Country);

public record CreateOrderRequest(
    string ShippingStreet,
    string ShippingCity,
    string ShippingPostalCode,
    string ShippingCountry);

