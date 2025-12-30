using FluentResults;
using Mapster;
using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Mapping;
using Ordering.Domain.Aggregates;
using TheShop.SharedKernel;

namespace Ordering.Application.Queries;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, Result<IReadOnlyList<OrderDto>>>
{
    private readonly IRepository<Order> _orderRepository;

    static GetUserOrdersQueryHandler()
    {
        OrderMappingConfig.Configure();
    }

    public GetUserOrdersQueryHandler(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<IReadOnlyList<OrderDto>>> Handle(GetUserOrdersQuery request, CancellationToken ct)
    {
        var orders = await _orderRepository.FindAsync(o => o.BuyerId == request.BuyerId, ct);

        var dtos = orders.Adapt<List<OrderDto>>();

        return Result.Ok<IReadOnlyList<OrderDto>>(dtos);
    }
}

