using FluentResults;
using Mapster;
using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Mapping;
using Ordering.Domain.Aggregates;
using TheShop.SharedKernel;

namespace Ordering.Application.Queries;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    private readonly IRepository<Order> _orderRepository;

    static GetOrderQueryHandler()
    {
        OrderMappingConfig.Configure();
    }

    public GetOrderQueryHandler(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);

        if (order is null)
        {
            return Result.Fail<OrderDto>("Order not found");
        }

        return Result.Ok(order.Adapt<OrderDto>());
    }
}

