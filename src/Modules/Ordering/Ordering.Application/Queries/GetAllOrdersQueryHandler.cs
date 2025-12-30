using FluentResults;
using Mapster;
using MediatR;
using Ordering.Application.Dtos;
using Ordering.Application.Mapping;
using Ordering.Domain.Aggregates;
using TheShop.SharedKernel;

namespace Ordering.Application.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<OrderDto>>>
{
    private readonly IRepository<Order> _orderRepository;

    static GetAllOrdersQueryHandler()
    {
        OrderMappingConfig.Configure();
    }

    public GetAllOrdersQueryHandler(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<PagedResult<OrderDto>>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var allOrders = await _orderRepository.GetAllAsync(ct);

        var totalCount = allOrders.Count;
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var orders = allOrders
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = orders.Adapt<List<OrderDto>>();

        return Result.Ok(new PagedResult<OrderDto>(dtos, request.Page, request.PageSize, totalCount, totalPages));
    }
}

