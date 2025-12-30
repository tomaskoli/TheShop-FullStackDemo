using FluentResults;
using MediatR;
using Ordering.Application.Dtos;

namespace Ordering.Application.Commands;

public record CreateOrderCommand(
    Guid BuyerId,
    string ShippingStreet,
    string ShippingCity,
    string ShippingPostalCode,
    string ShippingCountry) : IRequest<Result<OrderDto>>;

